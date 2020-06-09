using CommonUtils.Serializer;
using CommonUtils.Validation;
using log4net;
using RabbitMQ.Client;
using RabbitMQWrapper.Configuration;
using RabbitMQWrapper.Connection;
using RabbitMQWrapper.Factories;
using RabbitMQWrapper.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;

namespace RabbitMQWrapper.Publisher
{
    public sealed class QueuePublisher<T> : IQueuePublisher<T> where T : class
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(QueuePublisher<>));
        private readonly IQueueConnectionFactory _connectionFactory;
        private readonly IJsonSerializer _serializer;
        private readonly IValidationHelper _validationHelper;
        private readonly string _publisherName;
        private readonly IPublisherConfiguration _publisherConfig;
        private readonly CancellationToken _cancellationToken;
        private bool _connected;
        private readonly object _lock = new object();
        private IConnectionHandler _connection;
        private IModel _channel;

        public QueuePublisher(IQueueConfiguration queueConfiguration,
                              IQueueConnectionFactory connectionFactory,
                              IJsonSerializer serializer,
                              IValidationHelper validationHelper,
                              string publisherName,
                              CancellationToken cancellationToken)
        {
            if (queueConfiguration == null)
                throw new ArgumentNullException(nameof(queueConfiguration));

            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _publisherName = publisherName ?? throw new ArgumentNullException(nameof(publisherName));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _validationHelper = validationHelper ?? throw new ArgumentNullException(nameof(validationHelper));
            _cancellationToken = cancellationToken;

            // verify that the queue configuration is valid
            if (!queueConfiguration.IsValid)
                throw new ArgumentException("Queue Configuration is not valid", nameof(queueConfiguration));

            // retrieve the specific queues configuration
            _publisherConfig = queueConfiguration.Publishers?.FirstOrDefault(c => c.Name == _publisherName);

            if (_publisherConfig == null)
                throw new ArgumentNullException(nameof(_publisherConfig));
        }

        public void Publish(T message)
        {
            Publish(message, null, null);
        }

        public void Publish(T message, string dynamicRoutingKey)
        {
            Publish(message, null, dynamicRoutingKey);
        }

        public void Publish(T message, IDictionary<string, object> headers)
        {
            Publish(message, headers, null);
        }

        public void Publish(T message, IDictionary<string, object> headers, string dynamicRoutingKey)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                // Validate message
                string validationErrors;
                if (!_validationHelper.TryValidate(message, out validationErrors))
                    throw new ValidationException(validationErrors);

                // serialise object...
                string messageBody = _serializer.SerializeObject(message);

                // Determine routing key
                var routingKey = dynamicRoutingKey ?? _publisherConfig.RoutingKey;

                _logger.DebugFormat(Resources.PublishingMessageLogEntry, _publisherConfig.ExchangeName, routingKey, messageBody);

                lock (_lock)
                {
                    if (!_connected)
                    {
                        _connection = _connectionFactory.CreateConnection(_publisherConfig.Name, _cancellationToken);

                        _channel = _connection.CreateModel();

                        _connected = true;

                    }
                }

                // Create message properties
                var basicProperties = CreateBasicProperties(headers);

                _channel.BasicPublish(_publisherConfig.ExchangeName,
                                     routingKey,
                                     true,
                                     basicProperties,
                                     Encoding.UTF8.GetBytes(messageBody));

                _logger.Info($"Sent message");

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

        private IBasicProperties CreateBasicProperties(IDictionary<string, object> headers)
        {
            var basicProperties = _channel.CreateBasicProperties();
            basicProperties.AppId = $"{GlobalContext.Properties["COMPONENT-NAME"]}_{Environment.MachineName}";

            if (_serializer.GetType() == typeof(XmlSerializer))
            {
                basicProperties.ContentType = "application/xml";
            }
            else
            {
                basicProperties.ContentType = "application/json";
            }

            if (headers != null)
            {
                basicProperties.Headers = headers;
            }

            basicProperties.Persistent = _publisherConfig.PublishesPersistentMessages;
            basicProperties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            return basicProperties;
        }
    }
}
