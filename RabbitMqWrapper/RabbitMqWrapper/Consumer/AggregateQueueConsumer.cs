using CommonUtils.Serializer;
using CommonUtils.Validation;
using log4net;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqWrapper.Configuration;
using RabbitMqWrapper.Connection;
using RabbitMqWrapper.Enumerations;
using RabbitMqWrapper.Factories;
using RabbitMqWrapper.Model;
using RabbitMqWrapper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMqWrapper.Consumer
{
    public abstract class AggregateQueueConsumer<TMessage, TGroup> : IDisposable where TMessage : class where TGroup : struct
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(AggregateQueueConsumer<,>));
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

        /// <summary>
        /// The message acknowledgement strategy for this Event Listener.
        /// </summary>
        protected virtual AcknowledgeBehaviour Behaviour => AcknowledgeBehaviour.AfterProcess;

        /// <summary>
        /// By group a the message aggregate
        /// </summary>
        private readonly IDictionary<TGroup, MessageAggregate<TMessage>> messageAggregateByGroup;
        /// <summary>
        /// This lock must be obtained when accessing messageAggregateByGroup
        /// </summary>
        private readonly object accessMessageAggregates;

        public AggregateQueueConsumer(IQueueConfiguration queueConfiguration,
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

            messageAggregateByGroup = new Dictionary<TGroup, MessageAggregate<TMessage>>();
            accessMessageAggregates = new object();

            this.performanceLoggingMethodName = GetType().Name + "." + nameof(Run);
        }

        public void Run(Func<TMessage, ulong, CancellationToken, string, Task> onMessage)
        {
            try
            {
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

                            if (!_cancellationToken.IsCancellationRequested)
                            {
                                var group = await GetAggregationGroup(message);

                                if (group.Success)
                                {
                                    lock (accessMessageAggregates)
                                    {
                                        if (!messageAggregateByGroup.ContainsKey(group.Group))
                                        {
                                            // set up the max time
                                            DateTime maxFutureFireTime = DateTime.Now.AddTicks(MaxTimeoutTimeSpan.Ticks);
                                            if (MaxTimeoutTimeSpan.Ticks <= 0)
                                            {
                                                maxFutureFireTime = DateTime.MaxValue;
                                            }

                                            // set up the timer
                                            var timer = new Timer(o => ProcessAggregation(group.Group, cancellationToken), null, TimeoutTimeSpan, TimeoutTimeSpan);

                                            // set up the message list
                                            var messages = new Dictionary<ulong, TMessage>();
                                            messages.Add(deliveryTag, message);

                                            messageAggregateByGroup.Add(group.Group, new MessageAggregate<TMessage>
                                            {
                                                MaxTimeout = maxFutureFireTime,
                                                Timer = timer,
                                                Messages = messages
                                            });

                                            _logger.Debug($"Adding new aggregation group '{group}' with timer set for {TimeoutTimeSpan} milliseconds from {DateTime.UtcNow.ToString()}");
                                        }
                                        else
                                        {
                                            // add the items to the existing group
                                            messageAggregateByGroup[group.Group].Messages.Add(deliveryTag, message);

                                            if (DateTime.Now < messageAggregateByGroup[group.Group].MaxTimeout)
                                            {
                                                messageAggregateByGroup[group.Group].Timer.Change(TimeoutTimeSpan, TimeoutTimeSpan);
                                                _logger.Debug($"Added to aggregation group '{group}' with timer reset for {TimeoutTimeSpan} milliseconds from {DateTime.UtcNow.ToString()}");
                                            }
                                            else
                                            {
                                                _logger.Debug($"Maxtimeout reached for group '{group}'");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    _channel.BasicAck(deliveryTag, false);
                                    _logger.Warn($"Failed to get group for message with delivery tag '{deliveryTag}'. This will not be aggregated.");
                                }
                            }
                            else
                            {
                                DisposeOfGroups();
                            }
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

        /// <summary>
        /// Processses the messages that have been grouped together. Acknowledging them on the queue.
        /// </summary>
        /// <param name="group">The group under which the messages need to be processed</param>
        private void ProcessAggregation(TGroup group, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                IEnumerable<TMessage> messages = new List<TMessage>();
                IEnumerable<ulong> deliveryTags = new List<ulong>();

                // take a copy of the messages and clear the dictionaries of this group - ensures future messages go into a new aggregate
                lock (accessMessageAggregates)
                {
                    if (messageAggregateByGroup.ContainsKey(group))
                    {
                        // stop the timer - avoid it firing again whilst in this method
                        messageAggregateByGroup[group].Timer.Change(Timeout.Infinite, Timeout.Infinite);

                        // get the data from the dictionaries
                        messages = messageAggregateByGroup[group].Messages.Select(t => t.Value);
                        deliveryTags = messageAggregateByGroup[group].Messages.Select(t => t.Key);

                        // dispose and clean up the timer
                        messageAggregateByGroup[group].Timer.Dispose();

                        // remove the message aggregate 
                        messageAggregateByGroup.Remove(group);
                    }
                }

                // process the messages
                bool success = false;
                if (messages.Any())
                {
                    try
                    {
                        success = TryProcessAggregationGroup(messages, group, cancellationToken).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to process aggregation group: '{group}'.", ex);
                    }
                }
                else
                {
                    _logger.Warn($"no messages for group '{group}' were found, this should never happen!");
                }

                // acknowledge the messages
                foreach (ulong deliveryTag in deliveryTags)
                {
                    if (success)
                    {
                        _channel.BasicAck(deliveryTag, false);
                        _logger.Info($"Acknowledging message with delivery tag '{deliveryTag}'");
                    }
                    else
                    {

                        _channel.BasicNack(deliveryTag, false, false);
                        _logger.Info($"Negatively Acknowledging message with delivery tag '{deliveryTag}'");
                    }
                }
            }
            else
            {
                DisposeOfGroups();
            }
        }

        /// <summary>
        /// Disposes of the timers and the data.
        /// </summary>
        private void DisposeOfGroups()
        {
            // dispose of all the timers and items
            lock (accessMessageAggregates)
            {
                if (messageAggregateByGroup.Keys.Any())
                {
                    foreach (var key in messageAggregateByGroup.Keys)
                    {
                        messageAggregateByGroup[key].Timer.Change(Timeout.Infinite, Timeout.Infinite);
                        messageAggregateByGroup[key].Timer.Dispose();
                    }
                    messageAggregateByGroup.Clear();
                }
            }

            _logger.Info($"Disposed of timers and data");
        }

        public void Dispose()
        {
            DisposeOfGroups();
        }
    }
}