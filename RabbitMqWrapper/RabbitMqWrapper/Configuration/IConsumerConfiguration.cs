namespace RabbitMqWrapper.Configuration
{
    public interface IConsumerConfiguration
    {
        string ExchangeName { get; set; }
        int MessageWaitTimeoutMilliseconds { get; set; }
        string Name { get; set; }
        string QueueName { get; set; }
        string RoutingKey { get; set; }
    }
}