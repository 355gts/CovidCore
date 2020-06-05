using CommonUtils.Exceptions;
using CommonUtils.Logging;
using log4net;
using RabbitMQWrapper.Consumer;
using RabbitMQWrapper.Enumerations;
using RabbitMQWrapper.Model;
using RabbitMQWrapper.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQWrapper.EventListeners
{
    public abstract class SequentialProcessingEventListener<TMessage> : IEventListener where TMessage : class
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(SequentialProcessingEventListener<>));

        private readonly IQueueConsumer<TMessage> _queueConsumer;
        private readonly string performanceLoggingMethodName;
        private readonly ConcurrentDictionary<string, ProcessingQueue<TMessage>> _processingQueues;
        private readonly ConcurrentDictionary<string, Task> _tasks;

        public SequentialProcessingEventListener(IQueueConsumer<TMessage> queueConsumer)
        {
            _queueConsumer = queueConsumer ?? throw new ArgumentNullException(nameof(queueConsumer));

            _processingQueues = new ConcurrentDictionary<string, ProcessingQueue<TMessage>>();
            _tasks = new ConcurrentDictionary<string, Task>();
            this.performanceLoggingMethodName = GetType().Name + "." + nameof(ProcessMessageAsync);
        }

        /// <summary>
        /// The message acknowledgement strategy for this Event Listener.
        /// </summary>
        protected virtual AcknowledgeBehaviour Behaviour => AcknowledgeBehaviour.AfterProcess;

        protected virtual string GetProcessingSequenceIdentifier(string routingKey)
        {
            if (string.IsNullOrWhiteSpace(routingKey))
                throw new ArgumentNullException(nameof(routingKey));

            string[] routingKeyParts = routingKey.Split('.');
            return routingKeyParts[0];
        }

        protected abstract Task ProcessMessageAsync(TMessage message, ulong deliveryTag, CancellationToken cancellationToken, string routingKey = null);

        private async Task MessageReceivedAsync(QueueMessage<TMessage> message, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested && !_tasks.Any(t => t.Value.IsFaulted))
            {
                if (Behaviour == AcknowledgeBehaviour.BeforeProcess)
                    _queueConsumer.AcknowledgeMessage(message.DeliveryTag);

                string processingSequenceIdentifier = GetProcessingSequenceIdentifier(message.RoutingKey);

                if (_processingQueues.ContainsKey(processingSequenceIdentifier))
                {
                    // Add a message to the processing queue, and signal the processing thread to alert it to the new message.
                    var processingQueue = _processingQueues[processingSequenceIdentifier];
                    processingQueue.Queue.Enqueue(message);
                    processingQueue.AutoResetEvent.Set();
                }
                else
                {
                    // create a new processing queue and kick off a task to process it.
                    var processingQueue = new ProcessingQueue<TMessage>();
                    processingQueue.Queue.Enqueue(message);
                    _processingQueues[processingSequenceIdentifier] = processingQueue;
                    var t = Task.Run(RunSequentialProcessor(processingQueue, cancellationToken));
                    _tasks.TryAdd(processingSequenceIdentifier, t);
                }

                _logger.Info($"Received new message {message.DeliveryTag}, processor queue length {_processingQueues.First().Value.Queue.Count()}");
            }

            // Remove completed queues
            var processingQueuesToRemove = new List<string>();
            foreach (var processingQueue in _processingQueues)
            {
                if (!processingQueue.Value.Queue.Any())
                {
                    processingQueue.Value.AutoResetEvent.Set();
                    processingQueuesToRemove.Add(processingQueue.Key);
                }
            }

            foreach (var processingQueueToRemove in processingQueuesToRemove)
            {
                _processingQueues.TryRemove(processingQueueToRemove, out _);
                _tasks.TryRemove(processingQueueToRemove, out _);
            }

            // TODO: is it possible to remove executed tasks from the Task list and processingqueues from the processing queues

            _logger.Info($"Number of tasks {_tasks.Count()}, number of queues {_processingQueues.Count()}.");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private Func<Task> RunSequentialProcessor(ProcessingQueue<TMessage> processingQueue, CancellationToken cancellationToken)
        {
            return async () =>
            {
                QueueMessage<TMessage> dequeuedMessage;
                while (!cancellationToken.IsCancellationRequested && processingQueue.Queue.TryPeek(out dequeuedMessage))
                {
                    using (var performanceLogger = new PerformanceLogger(performanceLoggingMethodName))
                    {
                        _logger.DebugFormat(Resources.MessageReceivedLogEntry, dequeuedMessage.DeliveryTag);

                        try
                        {
                            await ProcessMessageAsync(
                                dequeuedMessage.Message,
                                dequeuedMessage.DeliveryTag,
                                cancellationToken,
                                dequeuedMessage.RoutingKey);

                            if (Behaviour != AcknowledgeBehaviour.Async)
                                _queueConsumer.AcknowledgeMessage(dequeuedMessage.DeliveryTag);

                            _logger.InfoFormat(Resources.MessageProcessedLogEntry, dequeuedMessage.DeliveryTag);
                        }
                        catch (FatalErrorException e)
                        {
                            _logger.Fatal(Resources.FatalErrorLogEntry, e);
                            _queueConsumer.NegativelyAcknowledgeAndRequeue(dequeuedMessage.DeliveryTag);
                            throw;
                        }
                        catch (Exception e)
                        {
                            _logger.ErrorFormat(Resources.ProcessingErrorLogEntry, dequeuedMessage.DeliveryTag, e);
                            _queueConsumer.NegativelyAcknowledge(dequeuedMessage.DeliveryTag);
                        }
                    }

                    // we have finished processing - remove the message from the queue and wait for the next one.
                    processingQueue.Queue.TryDequeue(out dequeuedMessage);
                    await processingQueue.AutoResetEvent.WaitOneAsync();
                }
            };
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
