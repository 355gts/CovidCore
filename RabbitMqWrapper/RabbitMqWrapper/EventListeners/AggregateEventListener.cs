using log4net;
using RabbitMQWrapper.Consumer;
using RabbitMQWrapper.Enumerations;
using RabbitMQWrapper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQWrapper.EventListeners
{
    public abstract class AggregateEventListener<TMessage, TGroup> : EventListener<TMessage>, IDisposable where TMessage : class where TGroup : struct
    {
        #region Private Fields
        private readonly ILog logger = LogManager.GetLogger(typeof(AggregateEventListener<TMessage, TGroup>));

        /// <summary>
        /// By group a the message aggregate
        /// </summary>
        private readonly IDictionary<TGroup, MessageAggregate<TMessage>> messageAggregateByGroup;
        /// <summary>
        /// This lock must be obtained when accessing messageAggregateByGroup
        /// </summary>
        private readonly object messageAggregatesLock = new object();
        #endregion

        #region Constructors
        protected AggregateEventListener(IQueueConsumer<TMessage> queueConsumer)
            : base(queueConsumer)
        {
            messageAggregateByGroup = new Dictionary<TGroup, MessageAggregate<TMessage>>();
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Attempt to retrieve the group this message needs to be aggregated under
        /// </summary>
        /// <param name="message">The message to find the aggregation group for</param>
        /// <returns>QueueGroup indicating whether the action was a sucess and the group</returns>
        protected abstract Task<QueueGroup<TGroup>> GetAggregationGroup(TMessage message);

        /// <summary>
        /// Processes all the messages in an aggregation.
        /// Note: A message will only be processed once i.e. it will not appear in another batch. It is possible for a future batches to complete before the current one if processing takes longer than the Timeout.
        /// A quick fix would be to increase the Timeout. The long fix would be to refactor this code, inspire yourself from the Sequential Event Listener.
        /// </summary>
        /// <param name="messages">in an aggregation</param>
        /// <param name="group">the messages were aggregated under</param>
        /// <returns></returns>
        protected abstract Task<bool> TryProcessAggregationGroup(IEnumerable<TMessage> messages, TGroup group, CancellationToken cancellationToken);

        /// <summary>
        /// The time, the aggregator should wait after the last message of that group is received before processing.
        /// </summary>
        protected abstract TimeSpan TimeoutTimeSpan { get; }

        /// <summary>
        /// Once this time has elapsed, processing must take place. If null then it does not apply
        /// </summary>
        protected abstract TimeSpan MaxTimeoutTimeSpan { get; }
        #endregion

        #region Overrides
        // set acknowledgement behaviour to never to acknowledgement is deferred until the messages are processed
        protected override AcknowledgeBehaviour Behaviour => AcknowledgeBehaviour.Never;

        protected override async Task ProcessMessageAsync(TMessage message, ulong deliveryTag, CancellationToken cancellationToken, string routingKey = null)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                var group = await GetAggregationGroup(message);

                if (!group.Success)
                {
                    logger.Warn($"Failed to get group for message with delivery tag '{deliveryTag}'. This will not be aggregated.");
                    return;
                }

                lock (messageAggregatesLock)
                {
                    if (!messageAggregateByGroup.ContainsKey(group.Group))
                    {
                        // set up the max time
                        DateTime maxFutureFireTime = DateTime.Now.AddTicks(MaxTimeoutTimeSpan.Ticks);
                        if (MaxTimeoutTimeSpan.Ticks <= 0)
                        {
                            maxFutureFireTime = DateTime.MaxValue;
                        }

                        // set up the timer
                        var timer = new Timer(o => ProcessAggregation(group.Group, cancellationToken), null, TimeoutTimeSpan, TimeoutTimeSpan);

                        // set up the message list
                        var messages = new Dictionary<ulong, TMessage>();
                        messages.Add(deliveryTag, message);

                        messageAggregateByGroup.Add(group.Group, new MessageAggregate<TMessage>
                        {
                            MaxTimeout = maxFutureFireTime,
                            Timer = timer,
                            Messages = messages
                        });

                        logger.Debug($"Adding new aggregation group '{group}' with timer set for {TimeoutTimeSpan} milliseconds from {DateTime.UtcNow.ToString()}");
                    }
                    else
                    {
                        // add the items to the existing group
                        messageAggregateByGroup[group.Group].Messages.Add(deliveryTag, message);

                        if (DateTime.Now < messageAggregateByGroup[group.Group].MaxTimeout)
                        {
                            messageAggregateByGroup[group.Group].Timer.Change(TimeoutTimeSpan, TimeoutTimeSpan);
                            logger.Debug($"Added to aggregation group '{group}' with timer reset for {TimeoutTimeSpan} milliseconds from {DateTime.UtcNow.ToString()}");
                        }
                        else
                        {
                            logger.Debug($"Maxtimeout reached for group '{group}'");
                        }
                    }
                }

            }
            else
            {
                DisposeOfGroups();
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Processses the messages that have been grouped together. Acknowledging them on the queue.
        /// </summary>
        /// <param name="group">The group under which the messages need to be processed</param>
        private void ProcessAggregation(TGroup group, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                IEnumerable<TMessage> messages = new List<TMessage>();
                IEnumerable<ulong> deliveryTags = new List<ulong>();

                // take a copy of the messages and clear the dictionaries of this group - ensures future messages go into a new aggregate
                lock (messageAggregatesLock)
                {
                    if (messageAggregateByGroup.ContainsKey(group))
                    {
                        // stop the timer - avoid it firing again whilst in this method
                        messageAggregateByGroup[group].Timer.Change(Timeout.Infinite, Timeout.Infinite);

                        // get the data from the dictionaries
                        messages = messageAggregateByGroup[group].Messages.Select(t => t.Value);
                        deliveryTags = messageAggregateByGroup[group].Messages.Select(t => t.Key);

                        // dispose and clean up the timer
                        messageAggregateByGroup[group].Timer.Dispose();

                        // remove the message aggregate 
                        messageAggregateByGroup.Remove(group);
                    }
                }

                // process the messages
                bool success = false;
                if (messages.Any())
                {
                    try
                    {
                        success = TryProcessAggregationGroup(messages, group, cancellationToken).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Failed to process aggregation group: '{group}'.", ex);
                    }
                }
                else
                {
                    logger.Warn($"no messages for group '{group}' were found, this should never happen!");
                }

                // acknowledge the messages
                foreach (ulong deliveryTag in deliveryTags)
                {
                    if (success)
                    {
                        _queueConsumer.AcknowledgeMessage(deliveryTag);
                        logger.Info($"Acknowledging message with delivery tag '{deliveryTag}'");
                    }
                    else
                    {
                        _queueConsumer.NegativelyAcknowledge(deliveryTag);
                        logger.Info($"Negatively Acknowledging message with delivery tag '{deliveryTag}'");
                    }
                }


                logger.Info($"Messages processed successfully.");
            }
            else
            {
                DisposeOfGroups();
            }
        }

        /// <summary>
        /// Disposes of the timers and the data.
        /// </summary>
        private void DisposeOfGroups()
        {
            // dispose of all the timers and items
            lock (messageAggregatesLock)
            {
                if (messageAggregateByGroup.Keys.Any())
                {
                    foreach (var key in messageAggregateByGroup.Keys)
                    {
                        messageAggregateByGroup[key].Timer.Change(Timeout.Infinite, Timeout.Infinite);
                        messageAggregateByGroup[key].Timer.Dispose();
                    }
                    messageAggregateByGroup.Clear();
                }
            }

            logger.Info($"Disposed of timers and data");
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            DisposeOfGroups();
        }
        #endregion
    }
}
