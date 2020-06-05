using RabbitMQWrapper.Connection;
using System;
using System.Threading;

namespace RabbitMQWrapper.Factories
{
    public interface IQueueConnectionFactory : IDisposable
    {
        IConnectionHandler CreateConnection(string connectionName, CancellationToken cancellationToken);
    }
}
