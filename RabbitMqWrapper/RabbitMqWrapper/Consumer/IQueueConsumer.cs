using System;
using System.Threading.Tasks;

namespace RabbitMqWrapper.Consumer
{
    public interface IQueueConsumer<T> where T : class
    {
        void Consume(Func<T, ulong, string, Task> onMessage);
    }
}