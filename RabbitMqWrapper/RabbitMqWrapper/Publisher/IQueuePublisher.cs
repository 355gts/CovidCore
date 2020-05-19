namespace RabbitMqWrapper.Publisher
{
    public interface IQueuePublisher<T> where T : class
    {
        void Publish(T message, string routingKey = null);
    }
}