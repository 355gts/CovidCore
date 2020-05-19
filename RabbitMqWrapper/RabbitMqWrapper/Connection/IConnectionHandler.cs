using RabbitMQ.Client;
using System;

namespace RabbitMqWrapper.Connection
{
    public interface IConnectionHandler : IDisposable
    {
        IModel CreateModel();
    }
}
