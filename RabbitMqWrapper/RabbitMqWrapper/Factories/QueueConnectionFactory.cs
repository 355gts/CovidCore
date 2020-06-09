using CommonUtils.Certificates;
using CommonUtils.Exceptions;
using RabbitMQ.Client;
using RabbitMQWrapper.Configuration;
using RabbitMQWrapper.Connection;
using RabbitMQWrapper.Properties;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace RabbitMQWrapper.Factories
{
    public class QueueConnectionFactory : IQueueConnectionFactory
    {
        private bool isDisposed;
        private readonly ConnectionFactory _connectionFactory;
        private readonly IQueueConfiguration _queueWrapperConfig;
        private readonly ConcurrentDictionary<string, IConnectionHandler> _connections;
        private readonly object _lock = new object();


        public QueueConnectionFactory(
            ConnectionFactory connectionFactory,
            IQueueConfiguration queueWrapperConfig,
            ICertificateHelper certificateHelper)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _queueWrapperConfig = queueWrapperConfig ?? throw new ArgumentNullException(nameof(QueueConnectionFactory));

            if (certificateHelper == null)
                throw new ArgumentNullException(nameof(certificateHelper));

            // verify that the queue configuration is valid
            if (!_queueWrapperConfig.IsValid)
                throw new ArgumentException("Queue Configuration is not valid", nameof(_queueWrapperConfig));

            _connections = new ConcurrentDictionary<string, IConnectionHandler>();

            _connectionFactory = connectionFactory;
            _connectionFactory.AuthMechanisms = new[] { new ExternalMechanismFactory() };
            _connectionFactory.Uri = _queueWrapperConfig.Uri;
            _connectionFactory.Ssl.ServerName = queueWrapperConfig.Uri.Host;
            _connectionFactory.ContinuationTimeout = TimeSpan.FromSeconds(queueWrapperConfig.ProtocolTimeoutIntervalSeconds);
            _connectionFactory.HandshakeContinuationTimeout = TimeSpan.FromSeconds(queueWrapperConfig.ProtocolTimeoutIntervalSeconds);

            var certificateResult = !string.IsNullOrEmpty(queueWrapperConfig.CertificatePath)
                ? certificateHelper.TryLoadCertificate(queueWrapperConfig.ClientCertificateSubjectName, queueWrapperConfig.CertificatePath, queueWrapperConfig.CertificatePassword)
                : certificateHelper.TryFindCertificate(queueWrapperConfig.ClientCertificateSubjectName);

            if (!certificateResult.Success)
            {
                throw new FatalErrorException(
                    string.Format(Resources.CouldNotFindCertificateError, queueWrapperConfig.ClientCertificateSubjectName, certificateResult.Message));
            }
            var certificates = certificateResult.Certificates;

            _connectionFactory.Ssl.Certs = certificates;
            _connectionFactory.Ssl.Version = System.Security.Authentication.SslProtocols.Tls12;
            _connectionFactory.Ssl.Enabled = true;
            _connectionFactory.AutomaticRecoveryEnabled = queueWrapperConfig.AutomaticRecoveryEnabled;
            _connectionFactory.NetworkRecoveryInterval = TimeSpan.FromSeconds(queueWrapperConfig.NetworkRecoveryIntervalSeconds);
            _connectionFactory.RequestedHeartbeat = queueWrapperConfig.RabbitMQHeartbeatSeconds;
        }

        public IConnectionHandler CreateConnection(string connectionName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(connectionName))
                throw new ArgumentNullException(nameof(connectionName));

            lock (_lock)
            {
                // if the connection is disposed remove it so that it can be re-initalised
                if (_connections.ContainsKey(connectionName) && _connections[connectionName].IsDisposed)
                {
                    _connections.TryRemove(connectionName, out _);
                }

                if (!_connections.ContainsKey(connectionName))
                {
                    var _connectionHandler = new ConnectionHandler(connectionName, _connectionFactory.CreateConnection(), cancellationToken, _queueWrapperConfig.AutomaticRecoveryEnabled);

                    _connections.TryAdd(connectionName, _connectionHandler);
                }

                return _connections[connectionName];
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                if (_connections != null && _connections.Any())
                {
                    foreach (var connection in _connections)
                    {
                        lock (_lock)
                        {
                            connection.Value.Dispose();
                            _connections.TryRemove(connection.Key, out _);
                        }
                    }
                }
            }

            isDisposed = true;
        }
    }
}
