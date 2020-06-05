using RabbitMQ.Client;
using System;

namespace RabbitMQWrapper.Connection
{
    public interface IConnectionHandler : IDisposable
    {
        bool IsDisposed { get; set; }

        IModel CreateModel();
    }
}
