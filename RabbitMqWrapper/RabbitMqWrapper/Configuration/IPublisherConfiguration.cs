namespace RabbitMQWrapper.Configuration
{
    public interface IPublisherConfiguration
    {
        string ExchangeName { get; set; }
        string Name { get; set; }
        bool PublishesPersistentMessages { get; set; }
        string RoutingKey { get; set; }
    }
}