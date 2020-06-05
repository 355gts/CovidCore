using CommonUtils.Threading;
using System.Collections.Concurrent;

namespace RabbitMQWrapper.Model
{
    public sealed class ProcessingQueue<TMessage> where TMessage : class
    {
        public ConcurrentQueue<QueueMessage<TMessage>> Queue { get; set; }

        public AutoResetEventAsync AutoResetEvent { get; set; }

        public ProcessingQueue()
        {
            Queue = new ConcurrentQueue<QueueMessage<TMessage>>();
            AutoResetEvent = new AutoResetEventAsync();
        }
    }
}
