using RabbitMqWrapper.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMqWrapper.Consumer
{
    public interface IQueueConsumer<T> where T : class
    {
        void AcknowledgeMessage(ulong deliveryTag);
        void NegativelyAcknowledge(ulong deliveryTag);
        void NegativelyAcknowledgeAndRequeue(ulong deliveryTag);
        void Run(Func<QueueMessage<T>, CancellationToken, Task> onMessageReceived, CancellationToken cancellationToken);
    }
}