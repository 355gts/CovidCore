using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMqWrapper.Consumer
{
    public interface ISequentialQueueConsumer<T> where T : class
    {
        void Run(Func<T, ulong, CancellationToken, string, Task> onMessage);
    }
}