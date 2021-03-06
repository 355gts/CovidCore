﻿using Autofac;
using RabbitMQWrapper.Consumer;
using RabbitMQWrapper.Publisher;
using System;
using System.Threading;

namespace RabbitMQWrapper.Extensions
{
    public static class ContainerExtensions
    {
        public static ContainerBuilder RegisterQueueConsumer<TMessageType>(this ContainerBuilder containerBuilder, string consumerName, CancellationToken cancellationToken) where TMessageType : class
        {
            if (string.IsNullOrEmpty(consumerName))
                throw new ArgumentNullException(nameof(consumerName));

            containerBuilder.RegisterType<QueueConsumer<TMessageType>>()
                            .As<IQueueConsumer<TMessageType>>()
                            .WithParameter("consumerName", consumerName)
                            .WithParameter("cancellationToken", cancellationToken)
                            .SingleInstance();

            return containerBuilder;
        }

        public static ContainerBuilder RegisterQueuePublisher<TMessageType>(this ContainerBuilder containerBuilder, string publisherName, CancellationToken cancellationToken) where TMessageType : class
        {
            if (string.IsNullOrEmpty(publisherName))
                throw new ArgumentNullException(nameof(publisherName));

            containerBuilder.RegisterType<QueuePublisher<TMessageType>>()
                            .As<IQueuePublisher<TMessageType>>()
                            .WithParameter("publisherName", publisherName)
                            .WithParameter("cancellationToken", cancellationToken)
                            .SingleInstance();

            return containerBuilder;
        }
    }
}
