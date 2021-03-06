﻿using CommonUtils.Certificates;
using CommonUtils.Exceptions;
using Covid.Rabbit.Configuration;
using Covid.Rabbit.Connection;
using Covid.Rabbit.Properties;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Covid.Rabbit.Factories
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

            _connections = new ConcurrentDictionary<string, IConnectionHandler>();

            _connectionFactory = connectionFactory;
            _connectionFactory.AuthMechanisms = new[] { new ExternalMechanismFactory() };
            _connectionFactory.Uri = _queueWrapperConfig.Uri;
            _connectionFactory.Ssl.ServerName = queueWrapperConfig.Uri.Host;
            _connectionFactory.ContinuationTimeout = TimeSpan.FromSeconds(queueWrapperConfig.ProtocolTimeoutIntervalSeconds);
            _connectionFactory.HandshakeContinuationTimeout = TimeSpan.FromSeconds(queueWrapperConfig.ProtocolTimeoutIntervalSeconds);

            X509Certificate2Collection certificates;
            if (!certificateHelper.TryFindCertificate(queueWrapperConfig.ClientCertificateSubjectName, out certificates))
            {
                throw new FatalErrorException(
                    string.Format(Resources.CouldNotFindCertificateError, queueWrapperConfig.ClientCertificateSubjectName, certificates.Count));
            }

            _connectionFactory.Ssl.Certs = certificates;
            _connectionFactory.Ssl.Version = System.Security.Authentication.SslProtocols.Tls12;
            _connectionFactory.Ssl.Enabled = true;
            _connectionFactory.AutomaticRecoveryEnabled = queueWrapperConfig.AutomaticRecoveryEnabled;
            _connectionFactory.NetworkRecoveryInterval = TimeSpan.FromSeconds(queueWrapperConfig.NetworkRecoveryIntervalSeconds);
            _connectionFactory.RequestedHeartbeat = queueWrapperConfig.RabbitMQHeartbeatSeconds;

            //this.ConnectionShuttingDown = false;
            //this.cancellationToken = cancellationToken;
            //this.cancellationToken.Register(ConnectionCancelled);

        }

        public IConnectionHandler CreateConnection(string connectionName, CancellationToken cancellationToken)
        {
            lock (_lock)
            {
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
