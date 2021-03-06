﻿namespace RabbitMQWrapper.Configuration
{
    public interface IQueueConfig
    {
        string Exchange { get; set; }
        string Name { get; set; }
        string Queue { get; set; }
        string RoutingKey { get; set; }
    }
}