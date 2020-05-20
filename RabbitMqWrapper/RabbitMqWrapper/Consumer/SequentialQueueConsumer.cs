using CommonUtils.Exceptions;
using CommonUtils.Logging;
using CommonUtils.Serializer;
using CommonUtils.Threading;
using CommonUtils.Validation;
using log4net;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMqWrapper.Configuration;
using RabbitMqWrapper.Connection;
using RabbitMqWrapper.Enumerations;
using RabbitMqWrapper.Factories;
using RabbitMqWrapper.Model;
using RabbitMqWrapper.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMqWrapper.Consumer
{
    public class SequentialQueueConsumer<T> : ISequentialQueueConsumer<T> where T : class
    {
        private sealed class ProcessingQueue
        {
            public string Name { get; set; }
            public ConcurrentQueue<QueueMessage<T>> Queue { get; set; }

            public AutoResetEventAsync AutoResetEvent { get; set; }

            public ProcessingQueue()
            {
                Queue = new ConcurrentQueue<QueueMessage<T>>();
                AutoResetEvent = new AutoResetEventAsync();
            }
        }

        private readonly ILog _logger = LogManager.GetLogger(typeof(SequentialQueueConsumer<>));
        private readonly IQueueConnectionFactory _connectionFactory;
        private readonly IJsonSerializer _serializer;
        private readonly IValidationHelper _validationHelper;
        private readonly string _consumerName;
        private readonly IQueueConfiguration _queueConfiguration;
        private readonly IConsumerConfiguration _consumerConfig;
        private readonly CancellationToken _cancellationToken;
        private bool _connected;
        private readonly object _lock = new object();
        private IConnectionHandler _connection;
        private IModel _channel;
        private readonly string performanceLoggingMethodName;
        private readonly ConcurrentDictionary<string, ProcessingQueue> processingQueues;
        private readonly List<Task> tasks;

        /// <summary>
        /// The message acknowledgement strategy for this Event Listener.
        /// </summary>
        protected virtual AcknowledgeBehaviour Behaviour => AcknowledgeBehaviour.AfterProcess;

        public SequentialQueueConsumer(IQueueConfiguration queueConfiguration,
                             IQueueConnectionFactory connectionFactory,
                             IJsonSerializer serializer,
                             IValidationHelper validationHelper,
                             string consumerName,
                              CancellationToken cancellationToken)
        {
            _queueConfiguration = queueConfiguration ?? throw new ArgumentNullException(nameof(queueConfiguration));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _consumerName = consumerName ?? throw new ArgumentNullException(nameof(consumerName));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _validationHelper = validationHelper ?? throw new ArgumentNullException(nameof(validationHelper));

            if (cancellationToken == null)
                throw new ArgumentNullException(nameof(cancellationToken));

            _cancellationToken = cancellationToken;

            // retrieve the specific queues configuration
            _consumerConfig = queueConfiguration.Consumers?.FirstOrDefault(c => c.Name == _consumerName);

            if (_consumerConfig == null)
                throw new ArgumentNullException(nameof(_consumerConfig));

            this.performanceLoggingMethodName = GetType().Name + "." + nameof(Run);
        }

        protected virtual string GetProcessingSequenceIdentifier(string routingKey)
        {
            if (string.IsNullOrWhiteSpace(routingKey))
                throw new ArgumentNullException(nameof(routingKey));

            string[] routingKeyParts = routingKey.Split('.');
            return routingKeyParts[1];
        }

        public void Run(Func<T, ulong, CancellationToken, string, Task> onMessage)
        {
            try
            {
                var processingQueues = new ConcurrentDictionary<string, ProcessingQueue>();
                var tasks = new List<Task>();

                lock (_lock)
                {
                    if (!_connected)
                    {
                        _connection = _connectionFactory.CreateConnection(_consumerConfig.Name, _cancellationToken);

                        _channel = _connection.CreateModel();
                        _channel.BasicQos(0, _queueConfiguration.MessagePrefetchCount, false);

                        var consumer = new EventingBasicConsumer(_channel);
                        consumer.Received += async (model, ea) =>
                        {
                            var body = ea.Body;
                            var deliveryTag = ea.DeliveryTag;
                            var routingKey = ea.RoutingKey;

                            var message = _serializer.Deserialize<T>(Encoding.UTF8.GetString(body));

                            // Validate object
                            string validationErrors;
                            bool success = _validationHelper.TryValidate(message, out validationErrors);
                            if (!success)
                            {
                                _logger.ErrorFormat(
                                    Resources.MessageFailsValidationLogEntry,
                                    deliveryTag,
                                    _consumerConfig.QueueName,
                                    validationErrors);
                                _channel.BasicNack(deliveryTag, false, false);
                            }

                            var queueMessage = new QueueMessage<T>(message, deliveryTag, routingKey, null);

                            //while (!cancellationToken.IsCancellationRequested && !tasks.Any(t => t.IsFaulted))
                            //{
                            string processingSequenceIdentifier = GetProcessingSequenceIdentifier(routingKey);

                            if (processingQueues.ContainsKey(processingSequenceIdentifier))
                            {
                                // Add a message to the processing queue, and signal the processing thread to alert it to the new message.
                                var processingQueue = processingQueues[processingSequenceIdentifier];
                                processingQueue.Queue.Enqueue(queueMessage);
                                processingQueue.AutoResetEvent.Set();
                            }
                            else
                            {
                                // create a new processing queue and kick off a task to process it.
                                var processingQueue = new ProcessingQueue() { Name = processingSequenceIdentifier };
                                processingQueue.Queue.Enqueue(queueMessage);
                                processingQueues[processingSequenceIdentifier] = processingQueue;
                                var t = Task.Run(RunSequentialProcessor(processingQueue, _cancellationToken, onMessage));
                                tasks.Add(t);
                            }

                            _logger.Info($"Processing Queues: { processingQueues.Count()}");

                            // Remove completed queues
                            var processingQueuesToRemove = new List<string>();
                            foreach (var processingQueue in processingQueues)
                            {
                                if (!processingQueue.Value.Queue.Any())
                                {
                                    processingQueue.Value.AutoResetEvent.Set();
                                    processingQueuesToRemove.Add(processingQueue.Key);
                                }
                            }

                            foreach (var processingQueueToRemove in processingQueuesToRemove)
                            {
                                processingQueues.TryRemove(processingQueueToRemove, out _);
                            }

                            // Remove completed tasks
                            tasks.RemoveAll(x => x.IsCompleted);
                        };

                        var dynamicQueue = $"{_queueConfiguration.TemporaryQueueNamePrefix}_{Guid.NewGuid().ToString()}";
                        // if the queue is not specified in the config then create a dynamic queue and bind to the exchange
                        if (string.IsNullOrEmpty(_consumerConfig.QueueName))
                        {
                            var queueDeclareResult = _channel.QueueDeclare(dynamicQueue, true, true, true, null);
                            if (queueDeclareResult == null)
                            {
                                // TODO handle this result correctly
                            }

                            _channel.QueueBind(dynamicQueue, _consumerConfig.ExchangeName, _consumerConfig.RoutingKey);
                        }

                        _channel.BasicConsume(queue: !string.IsNullOrEmpty(_consumerConfig.QueueName) ? _consumerConfig.QueueName : dynamicQueue,
                                             autoAck: false,
                                             consumer: consumer);

                        _connected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"An unexpected exception occurred, error details '{ex.Message}'", ex);

                lock (_lock)
                {
                    _connected = false;
                }

                if (_channel != null)
                    _channel.Dispose();

                if (_connection != null)
                    _connection.Dispose();
            }
        }

        private Func<Task> RunSequentialProcessor(ProcessingQueue processingQueue, CancellationToken cancellationToken, Func<T, ulong, CancellationToken, string, Task> onMessage)
        {
            return async () =>
            {
                QueueMessage<T> dequeuedMessage;
                while (!cancellationToken.IsCancellationRequested && processingQueue.Queue.TryPeek(out dequeuedMessage))
                {
                    _logger.Info($"Processing Queue: {processingQueue.Name} Count: {processingQueue.Queue.Count()}");
                    using (var performanceLogger = new PerformanceLogger(performanceLoggingMethodName))
                    {
                        var message = dequeuedMessage.Message;
                        var deliveryTag = dequeuedMessage.DeliveryTag;
                        var routingKey = dequeuedMessage.RoutingKey;

                        try
                        {
                            if (Behaviour == AcknowledgeBehaviour.BeforeProcess)
                                _channel.BasicAck(deliveryTag, false);

                            await onMessage(message, deliveryTag, _cancellationToken, routingKey);

                            if (Behaviour == AcknowledgeBehaviour.AfterProcess)
                                _channel.BasicAck(deliveryTag, false);

                            if (Behaviour != AcknowledgeBehaviour.Never)
                                _logger.InfoFormat(Resources.MessageProcessedLogEntry, deliveryTag);
                        }
                        catch (AlreadyClosedException ex)
                        {
                            _logger.Warn($"The connection to Rabbit was closed while processing message with deliveryTag '{deliveryTag}', error details - '{ex.Message}'.");
                        }
                        catch (FatalErrorException e)
                        {
                            if (Behaviour == AcknowledgeBehaviour.AfterProcess
                             || Behaviour == AcknowledgeBehaviour.Async)
                                _channel.BasicNack(deliveryTag, false, false);

                            _logger.Fatal(Resources.FatalErrorLogEntry, e);
                            throw;
                        }
                        catch (Exception e)
                        {
                            _logger.ErrorFormat(Resources.ProcessingErrorLogEntry, deliveryTag, e);

                            if (Behaviour == AcknowledgeBehaviour.AfterProcess
                             || Behaviour == AcknowledgeBehaviour.Async)
                                _channel.BasicNack(deliveryTag, false, false);
                        }

                        // we have finished processing - remove the message from the queue and wait for the next one.
                        processingQueue.Queue.TryDequeue(out dequeuedMessage);
                        await processingQueue.AutoResetEvent.WaitOneAsync();
                    }
                };
            };
        }
    }
}