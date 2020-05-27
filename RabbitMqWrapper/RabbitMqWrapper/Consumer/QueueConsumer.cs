using CommonUtils.Serializer;
using CommonUtils.Validation;
using log4net;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqWrapper.Configuration;
using RabbitMqWrapper.Connection;
using RabbitMqWrapper.Factories;
using RabbitMqWrapper.Model;
using RabbitMqWrapper.Properties;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMqWrapper.Consumer
{
    public class QueueConsumer<T> : IQueueConsumer<T> where T : class
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(QueueConsumer<>));
        private readonly IQueueConnectionFactory _connectionFactory;
        private readonly IJsonSerializer _serializer;
        private readonly IValidationHelper _validationHelper;
        private readonly IQueueConfiguration _queueConfiguration;
        private readonly IConsumerConfiguration _consumerConfig;
        private readonly string _consumerName;
        private IConnectionHandler _connection;
        private IModel _channel;
        private bool _connected;
        private readonly object _lock = new object();
        private readonly object _channelLock = new object();
        private readonly string performanceLoggingMethodName;
        private string queueName;

        public QueueConsumer(IQueueConfiguration queueConfiguration,
                             IQueueConnectionFactory connectionFactory,
                             IJsonSerializer serializer,
                             IValidationHelper validationHelper,
                             string consumerName)
        {
            _queueConfiguration = queueConfiguration ?? throw new ArgumentNullException(nameof(queueConfiguration));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _validationHelper = validationHelper ?? throw new ArgumentNullException(nameof(validationHelper));

            if (string.IsNullOrEmpty(consumerName))
                throw new ArgumentNullException(nameof(consumerName));

            _consumerName = consumerName;

            // retrieve the specific queues configuration
            _consumerConfig = queueConfiguration.Consumers?.FirstOrDefault(c => c.Name == _consumerName);

            if (_consumerConfig == null)
                throw new ArgumentNullException(nameof(_consumerConfig));

            this.performanceLoggingMethodName = GetType().Name + "." + nameof(Run);
        }

        public void Run(Func<QueueMessage<T>, CancellationToken, Task> onMessageReceived, CancellationToken cancellationToken)
        {
            try
            {
                if (!_connected)
                {
                    _connection = _connectionFactory.CreateConnection(_consumerConfig.Name, cancellationToken);

                    _channel = _connection.CreateModel();
                    _channel.BasicQos(0, _queueConfiguration.MessagePrefetchCount, false);

                    var consumer = new EventingBasicConsumer(_channel);
                    consumer.Received += async (model, rabbitMessage) =>
                    {
                        try
                        {
                            // Deserialize object
                            T messageObject = _serializer.Deserialize<T>(Encoding.UTF8.GetString(rabbitMessage.Body));

                            // Validate object
                            string validationErrors;
                            bool success = _validationHelper.TryValidate(messageObject, out validationErrors);

                            if (!success)
                            {
                                _logger.ErrorFormat(
                                    Resources.MessageFailsValidationLogEntry,
                                    rabbitMessage.DeliveryTag,
                                    queueName,
                                    validationErrors);
                                NegativelyAcknowledge(rabbitMessage.DeliveryTag);
                                return;
                            }

                            _logger.InfoFormat(Resources.MessageSuccessfullyReceivedLogEntry, rabbitMessage.DeliveryTag, queueName);
                            var message = new QueueMessage<T>(messageObject,
                                rabbitMessage.DeliveryTag,
                                rabbitMessage.RoutingKey,
                                rabbitMessage.BasicProperties.Headers);

                            // call the event handler to process the message
                            await onMessageReceived(message, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.ErrorFormat(
                                Resources.MessageFailsValidationLogEntry,
                                rabbitMessage.DeliveryTag,
                                queueName,
                                ex.Message);

                            NegativelyAcknowledge(rabbitMessage.DeliveryTag);
                        }
                    };

                    var dynamicQueue = $"{_queueConfiguration.TemporaryQueueNamePrefix}_{Guid.NewGuid().ToString()}";
                    // if the queue is not specified in the config then create a dynamic queue and bind to the exchange
                    if (string.IsNullOrEmpty(_consumerConfig.QueueName))
                    {
                        var queueDeclareResult = _channel.QueueDeclare(dynamicQueue, true, true, true, null);
                        if (queueDeclareResult == null)
                        {
                            throw new IOException(Resources.TemporaryQueueCreationError);
                        }

                        queueName = queueDeclareResult.QueueName;
                        _channel.QueueBind(dynamicQueue, _consumerConfig.ExchangeName, _consumerConfig.RoutingKey);
                    }
                    else
                    {

                        queueName = _consumerConfig.QueueName;
                    }

                    _channel.BasicConsume(queue: queueName,
                                         autoAck: false,
                                         consumer: consumer);
                    _connected = true;
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

        public void AcknowledgeMessage(ulong deliveryTag)
        {
            lock (_channelLock)
            {
                // if we lose connection acknowledging a delivery tag that 
                // rabbit does not know about causes the client to stop the consumer
                // so check the connection is still alive
                //if (channelAvailableEvent.IsSet)
                //{
                if (_channel.IsOpen)
                {
                    _channel.BasicAck(deliveryTag, false);
                }
                //}
            }

            //_logger.DebugFormat(MessageAcknowledgedLogEntry, deliveryTag, queueName);
        }

        public void NegativelyAcknowledge(ulong deliveryTag)
        {
            try
            {
                lock (_channelLock)
                {
                    // if we lose connection acknowledging a delivery tag that 
                    // rabbit does not know about causes the client to stop the consumer
                    // so check the connection is still alive
                    //if (channelAvailableEvent.IsSet)
                    //{
                    if (_channel.IsOpen)
                    {
                        _channel.BasicNack(deliveryTag, false, false);
                    }
                    //}
                }

                //_logger.DebugFormat(MessageNegativelyAcknowledgedLogEntry, deliveryTag, queueName);
            }
            catch (Exception e)
            {
                // Do nothing - we don't mind that the nack has failed. Rabbit will fix this if necessary.
                //_logger.WarnFormat("Failed to negatively acknowledge message {0} from queue {1}. {2}", deliveryTag, queueName, e);
            }
        }

        public void NegativelyAcknowledgeAndRequeue(ulong deliveryTag)
        {
            lock (_channelLock)
            {
                // if we lose connection acknowledging a delivery tag that 
                // rabbit does not know about causes the client to stop the consumer
                // so check the connection is still alive
                //if (channelAvailableEvent.IsSet)
                //{
                if (_channel.IsOpen)
                {
                    _channel.BasicNack(deliveryTag, false, true);
                }
                //}
            }

            //_logger.DebugFormat(MessageRequeuedLogEntry, deliveryTag, queueName);
        }
    }
}