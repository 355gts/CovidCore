using System.Collections.Generic;

namespace RabbitMqWrapper.Publisher
{
    public interface IQueuePublisher<T> where T : class
    {
        void Publish(T message);
        void Publish(T message, IDictionary<string, object> headers);
        void Publish(T message, IDictionary<string, object> headers, string dynamicRoutingKey);
        void Publish(T message, string dynamicRoutingKey);
    }
}