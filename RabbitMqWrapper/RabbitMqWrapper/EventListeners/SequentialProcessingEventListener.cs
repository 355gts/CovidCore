using log4net;
using RabbitMqWrapper.Consumer;
using RabbitMqWrapper.Enumerations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMqWrapper.EventListeners
{
    public abstract class SequentialProcessingEventListener<T> where T : class
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(SequentialProcessingEventListener<>));

        private readonly ISequentialQueueConsumer<T> _queueConsumer;
        private readonly string performanceLoggingMethodName;

        public SequentialProcessingEventListener(ISequentialQueueConsumer<T> queueConsumer)
        {
            _queueConsumer = queueConsumer ?? throw new ArgumentNullException(nameof(queueConsumer));
            this.performanceLoggingMethodName = GetType().Name + "." + nameof(ProcessMessageAsync);


        }

        /// <summary>
        /// The message acknowledgement strategy for this Event Listener.
        /// </summary>
        protected virtual AcknowledgeBehaviour Behaviour => AcknowledgeBehaviour.AfterProcess;

        public abstract Task ProcessMessageAsync(T message, ulong deliveryTag, CancellationToken cancellationToken, string routingKey = null);

        public async Task Run()
        {
            // tell the consumer to start listening and then pass it the process message action to perform
            _queueConsumer.Run(ProcessMessageAsync);
        }
    }
}
