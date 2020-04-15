using Autofac;
using Covid.Common.Extensions;
using Covid.Common.HttpClientHelper;
using Covid.Common.HttpClientHelper.Configuration;
using Covid.Common.HttpClientHelper.Factories;
using Covid.Common.Mapper;
using Covid.CommonUtils.Serializers;
using Covid.Message.Model.Publisher;
using Covid.Message.Model.Users;
using Covid.Rabbit.Configuration;
using Covid.Rabbit.Connection;
using Covid.Rabbit.Extensions;
using Covid.Rabbit.Factories;
using Covid.$ext_safeprojectname$.EventListeners;
using Covid.$ext_safeprojectname$.Processors;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Covid.$ext_safeprojectname$.Container
{
    public static class ContainerConfiguration
    {
        public static IContainer Configure(
            IConfiguration configuration,
            CancellationTokenSource eventListenerCancellationTokenSource,
            CancellationTokenSource cancellationTokenSource)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (eventListenerCancellationTokenSource == null)
                throw new ArgumentNullException(nameof(eventListenerCancellationTokenSource));

            if (cancellationTokenSource == null)
                throw new ArgumentNullException(nameof(cancellationTokenSource));

            var containerBuilder = new ContainerBuilder();

            // add the configuration to the container
            containerBuilder.Register(ctx => { return configuration; }).As<IConfiguration>().SingleInstance();

            // register configuration sections
            containerBuilder.RegisterConfigurationSection<IQueueConfiguration, QueueConfiguration>(configuration, "queueConfiguration");
            containerBuilder.RegisterConfigurationSection<IEnumerable<IHttpClientConfiguration>, List<HttpClientConfiguration>>(configuration, "services");

            // load the assembly containing the mappers
            var executingAssembly = Assembly.Load("$ext_safeprojectname$");

            // register types
            containerBuilder.RegisterType<HttpClientHelper>().As<IHttpClientHelper>().WithParameter("serviceName", "covid").SingleInstance();
            containerBuilder.RegisterType<HttpClientFactory>().As<IHttpClientFactory>().SingleInstance();
            containerBuilder.RegisterType<CovidApiHelper>().As<ICovidApiHelper>().SingleInstance();
            containerBuilder.RegisterType<JsonSerializer>().As<IJsonSerializer>().SingleInstance();
            containerBuilder.RegisterType<Processor>().As<IProcessor>().SingleInstance();
            containerBuilder.RegisterType<QueueConnectionFactory>().As<IQueueConnectionFactory>().SingleInstance();
            containerBuilder.RegisterType<ConnectionHandler>().As<IConnectionHandler>().SingleInstance();
            containerBuilder.RegisterType<MessagePublisher>().As<IMessagePublisher>().SingleInstance();
            containerBuilder.Register(ctx => { return new Mapper(executingAssembly); }).As<IMapper>().SingleInstance();
            
			// event listeners
			//containerBuilder.RegisterType<ServiceEventListener>();

			// queue consumers
            //containerBuilder.RegisterQueueConsumer<TMessageType>("QueueConsumerNName", cancellationTokenSource.Token);
			
			// queue publishers
            //containerBuilder.RegisterQueuePublisher<TMessageType>("QueuePublisherName", cancellationTokenSource.Token);

            return containerBuilder.Build();
        }
    }
}
