using System;
using System.Collections.Generic;

namespace RabbitMQWrapper.Model
{
    public sealed class QueueMessage<T> where T : class
    {
        public T Message { get; }

        public ulong DeliveryTag { get; }

        public string RoutingKey { get; set; }

        public IDictionary<string, object> MessageHeaders { get; }

        public QueueMessage(T message, ulong deliveryTag, string routingKey, IDictionary<string, object> headers)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Message = message;
            DeliveryTag = deliveryTag;
            RoutingKey = routingKey;
            MessageHeaders = headers ?? new Dictionary<string, object>();
        }
    }
}
