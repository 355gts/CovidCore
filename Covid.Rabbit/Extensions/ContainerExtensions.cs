using Autofac;
using Covid.Rabbit.Consumer;
using Covid.Rabbit.Publisher;
using System;
using System.Threading;

namespace Covid.Rabbit.Extensions
{
    public static class ContainerExtensions
    {
        public static ContainerBuilder RegisterQueueConsumer<TMessageType>(this ContainerBuilder containerBuilder, string consumerName, CancellationToken cancellationToken) where TMessageType : class
        {
            if (string.IsNullOrEmpty(consumerName))
                throw new ArgumentNullException(nameof(consumerName));

            containerBuilder.RegisterType<QueueConsumer<TMessageType>>()
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
                            .WithParameter("publisherName", publisherName)
                            .WithParameter("cancellationToken", cancellationToken)
                            .SingleInstance();

            return containerBuilder;
        }
    }
}
