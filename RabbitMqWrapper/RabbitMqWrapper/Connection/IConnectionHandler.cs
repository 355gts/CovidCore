using RabbitMQ.Client;
using System;

namespace RabbitMqWrapper.Connection
{
    public interface IConnectionHandler : IDisposable
    {
        bool IsDisposed { get; set; }

        IModel CreateModel();
    }
}
