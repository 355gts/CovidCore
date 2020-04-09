using RabbitMQ.Client;
using System;

namespace Covid.Rabbit.Connection
{
    public interface IConnectionHandler : IDisposable
    {
        IModel CreateModel();
    }
}
