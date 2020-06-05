using System;
using System.Collections.Generic;
using System.Threading;

namespace RabbitMQWrapper.Model
{
    internal sealed class MessageAggregate<TMessage> where TMessage : class
    {
        public IDictionary<ulong, TMessage> Messages { get; set; }
        public DateTime MaxTimeout { get; set; }
        public Timer Timer { get; set; }
    }
}
