using Autofac;
using Microsoft.Extensions.Configuration;
using System;

namespace Covid.Common.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterConfigurationSection<TInterface, TType>(this ContainerBuilder containerBuilder, IConfiguration configuration, string sectionName) where TInterface : class where TType : class
        {
            var config = Activator.CreateInstance(typeof(TType));

            configuration.GetSection(sectionName).Bind(config);

            containerBuilder.Register(ctx => { return config; }).As<TInterface>().SingleInstance();

            return containerBuilder;
        }

        public static ContainerBuilder RegisterConfigurationSection<TType>(this ContainerBuilder containerBuilder, IConfiguration configuration, string sectionName) where TType : class
        {
            var config = Activator.CreateInstance(typeof(TType));

            configuration.GetSection(sectionName).Bind(config);

            containerBuilder.Register(ctx => { return config; }).SingleInstance();

            return containerBuilder;
        }
    }
}
