using Covid.Rabbit.Consumer;
using log4net;
using System;
using System.Threading.Tasks;

namespace Covid.Rabbit
{
    public abstract class EventListener<T> where T : class
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(EventListener<>));

        private readonly QueueConsumer<T> _queueConsumer;

        public EventListener(QueueConsumer<T> queueConsumer)
        {
            _queueConsumer = queueConsumer ?? throw new ArgumentNullException(nameof(queueConsumer));
        }

        public abstract Task ProcessMessageAsync(T message, ulong deliveryTag, string routingKey = null);

        public async Task Run()
        {
            // tell the consumer to start listening and then pass it the process message action to perform
            _queueConsumer.Consume(ProcessMessageAsync);
        }
    }
}
