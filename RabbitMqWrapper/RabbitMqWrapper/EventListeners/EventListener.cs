using CommonUtils.Exceptions;
using CommonUtils.Logging;
using log4net;
using RabbitMQ.Client.Exceptions;
using RabbitMQWrapper.Consumer;
using RabbitMQWrapper.Enumerations;
using RabbitMQWrapper.Model;
using RabbitMQWrapper.Properties;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQWrapper.EventListeners
{
    public abstract class EventListener<TMessage> : IEventListener where TMessage : class
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(EventListener<>));

        protected readonly IQueueConsumer<TMessage> _queueConsumer;
        private readonly string performanceLoggingMethodName;

        public EventListener(IQueueConsumer<TMessage> queueConsumer)
        {
            _queueConsumer = queueConsumer ?? throw new ArgumentNullException(nameof(queueConsumer));

            this.performanceLoggingMethodName = GetType().Name + "." + nameof(ProcessMessageAsync);
        }

        /// <summary>
        /// The message acknowledgement strategy for this Event Listener.
        /// </summary>
        protected virtual AcknowledgeBehaviour Behaviour => AcknowledgeBehaviour.AfterProcess;

        protected abstract Task ProcessMessageAsync(TMessage message, ulong deliveryTag, CancellationToken cancellationToken, string routingKey = null);

        private async Task MessageReceivedAsync(QueueMessage<TMessage> message, CancellationToken cancellationToken)
        {
            using (var performanceLogger = new PerformanceLogger(performanceLoggingMethodName))
            {
                try
                {
                    if (Behaviour == AcknowledgeBehaviour.BeforeProcess)
                        _queueConsumer.AcknowledgeMessage(message.DeliveryTag);

                    await ProcessMessageAsync(message.Message, message.DeliveryTag, cancellationToken, message.RoutingKey);

                    if (Behaviour == AcknowledgeBehaviour.AfterProcess)
                        _queueConsumer.AcknowledgeMessage(message.DeliveryTag);

                    if (Behaviour != AcknowledgeBehaviour.Never && Behaviour != AcknowledgeBehaviour.Deferred)
                        _logger.InfoFormat(Resources.MessageProcessedLogEntry, message.DeliveryTag);
                }
                catch (AlreadyClosedException ex)
                {
                    _logger.Warn($"The connection to Rabbit was closed while processing message with deliveryTag '{message.DeliveryTag}', error details - '{ex.Message}'.");
                }
                catch (FatalErrorException e)
                {
                    if (Behaviour == AcknowledgeBehaviour.AfterProcess
                     || Behaviour == AcknowledgeBehaviour.Async)
                        _queueConsumer.NegativelyAcknowledgeAndRequeue(message.DeliveryTag);

                    _logger.Fatal(Resources.FatalErrorLogEntry, e);
                }
                catch (Exception e)
                {
                    _logger.ErrorFormat(Resources.ProcessingErrorLogEntry, message.DeliveryTag, e);

                    if (Behaviour == AcknowledgeBehaviour.AfterProcess
                     || Behaviour == AcknowledgeBehaviour.Async)
                        _queueConsumer.NegativelyAcknowledge(message.DeliveryTag);
                }
            }
        }

        public void Run(CancellationToken cancellationToken)
        {
            if (cancellationToken == null)
                throw new ArgumentNullException(nameof(cancellationToken));

            // tell the consumer to start listening and then pass it the process message action to perform
            _queueConsumer.Run(MessageReceivedAsync, cancellationToken);
        }
    }
}
