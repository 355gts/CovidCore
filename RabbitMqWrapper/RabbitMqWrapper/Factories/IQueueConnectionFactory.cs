using RabbitMqWrapper.Connection;
using System;
using System.Threading;

namespace RabbitMqWrapper.Factories
{
    public interface IQueueConnectionFactory : IDisposable
    {
        IConnectionHandler CreateConnection(string connectionName, CancellationToken cancellationToken);
    }
}
