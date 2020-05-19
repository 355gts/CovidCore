using CommonUtils.Serializer;
using log4net;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMqWrapper.Configuration;
using RabbitMqWrapper.Connection;
using RabbitMqWrapper.Enumerations;
using RabbitMqWrapper.Factories;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMqWrapper.Consumer
{
    public class QueueConsumer<T> where T : class
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(QueueConsumer<>));
        private readonly IQueueConnectionFactory _connectionFactory;
        private readonly IJsonSerializer _serializer;
        private readonly string _consumerName;
        private readonly IQueueConfiguration _queueConfiguration;
        private readonly IConsumerConfiguration _consumerConfig;
        private readonly CancellationToken _cancellationToken;
        private bool _connected;
        private readonly object _lock = new object();
        private IConnectionHandler _connection;
        private IModel _channel;

        public virtual AcknowledgementBehaviour AcknowledgementBehaviour => AcknowledgementBehaviour.PostProcess;

        public QueueConsumer(IQueueConfiguration queueConfiguration,
                             IQueueConnectionFactory connectionFactory,
                             IJsonSerializer serializer,
                             string consumerName,
                              CancellationToken cancellationToken)
        {
            _queueConfiguration = queueConfiguration ?? throw new ArgumentNullException(nameof(queueConfiguration));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _consumerName = consumerName ?? throw new ArgumentNullException(nameof(consumerName));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

            if (cancellationToken == null)
                throw new ArgumentNullException(nameof(cancellationToken));

            _cancellationToken = cancellationToken;

            // retrieve the specific queues configuration
            _consumerConfig = queueConfiguration.Consumers?.FirstOrDefault(c => c.Name == _consumerName);

            if (_consumerConfig == null)
                throw new ArgumentNullException(nameof(_consumerConfig));

        }

        public void Consume(Func<T, ulong, string, Task> onMessage)
        {
            try
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

                        try
                        {
                            var message = _serializer.Deserialize<T>(Encoding.UTF8.GetString(body));

                            _logger.Info($"Received message");

                            if (AcknowledgementBehaviour == AcknowledgementBehaviour.PreProcess)
                                _channel.BasicAck(deliveryTag, false);

                            await onMessage(message, deliveryTag, routingKey);

                            _channel.BasicAck(deliveryTag, false);
                        }
                        catch (AlreadyClosedException ex)
                        {
                            _logger.Warn($"The connection to Rabbit was closed while processing message with deliveryTag '{deliveryTag}', error details - '{ex.Message}'.");
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn($"An Exception occurred processing message with deliveryTag '{deliveryTag}', error details - '{ex.Message}'.");
                            _channel.BasicNack(deliveryTag, false, false);
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

                    lock (_lock)
                    {
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
    }
}