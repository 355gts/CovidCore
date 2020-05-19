using log4net;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.Impl;
using RabbitMQ.Client.Impl;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMqWrapper.Connection
{
    public class ConnectionHandler : IConnectionHandler
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(ConnectionHandler));
        private bool isDisposed;
        private readonly string _connectionName;
        private IConnection _connection;
        private readonly CancellationToken _cancellationToken;
        private readonly bool _autoRecoveryEnabled;

        public ConnectionHandler(string connectionName, IConnection connection, CancellationToken cancellationToken, bool autoRecoveryEnabled)
        {
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));

            if (cancellationToken == null)
                throw new ArgumentNullException(nameof(cancellationToken));

            _cancellationToken = cancellationToken;
            _autoRecoveryEnabled = autoRecoveryEnabled;

            _connection.CallbackException += OnCallbackException;
            _connection.ConnectionBlocked += OnConnectionBlocked;
            _connection.ConnectionRecoveryError += OnConnectionRecoveryError;
            _connection.ConnectionShutdown += OnConnectionShutdown;
            _connection.ConnectionUnblocked += OnConnectionUnblocked;
            _connection.RecoverySucceeded += OnRecoverySucceeded;
        }

        private void OnRecoverySucceeded(object sender, EventArgs e)
        {
            _logger.Info($"Successfully recovered connection to Rabbit.");
        }

        private void OnConnectionUnblocked(object sender, EventArgs e)
        {
            _logger.Info($"Connection to Rabbit is currently blocked.");
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            if (e.Initiator == ShutdownInitiator.Application || _cancellationToken.IsCancellationRequested)
            {
                _logger.Info("Shutting down connection.");
                return;
            }

            _logger.Warn($"Received ModelShutdown event from initiator '{e.Initiator.ToString()}'.");
        }

        private void OnConnectionRecoveryError(object sender, ConnectionRecoveryErrorEventArgs e)
        {
            _logger.Warn($"Failed to auto recover connection to Rabbit, re-attempting.....");
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            _logger.Warn($"The connection to Rabbit is currently blocked due to '{e.Reason}'.");
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _logger.Error($"An unexpected callback exception has occurred, error details - '{e.Exception.Message}'.", e.Exception);
        }

        public IModel CreateModel()
        {
            var channel = _connection.CreateModel();
            channel.ModelShutdown += (sender, args) =>
            {
                if (args.Initiator == ShutdownInitiator.Application || _cancellationToken.IsCancellationRequested)
                {
                    _logger.Info("Shutting down connection.");
                    return;
                }

                if (!_autoRecoveryEnabled)
                {
                    _logger.Warn($"Auto recovery is not enabled, connection to Rabbit has been closed by '{args.Initiator.ToString()}'.");
                    return;
                }

                _logger.Warn($"Received ModelShutdown event from initiator '{args.Initiator.ToString()}', attempting to auto recover.");

                Task.Run(() => ((AutorecoveringModel)channel).AutomaticallyRecover((AutorecoveringConnection)_connection, null));
            };

            return channel;
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
                if (_connection != null)
                    _connection.Dispose();
            }

            isDisposed = true;
        }
    }
}
