using Covid.Rabbit.Connection;
using System;
using System.Threading;

namespace Covid.Rabbit.Factories
{
    public interface IQueueConnectionFactory : IDisposable
    {
        IConnectionHandler CreateConnection(string connectionName, CancellationToken cancellationToken);
    }
}
