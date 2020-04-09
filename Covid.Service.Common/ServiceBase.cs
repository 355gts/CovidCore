using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;
using System.Xml;


namespace Covid.Service.Common
{
    public class ServiceBase
    {
        private const string log4netName = "log4net";
        private const string log4netFilename = "log4net.config";

        private readonly ILog _logger = LogManager.GetLogger(typeof(ServiceBase));

        protected IConfiguration Configuration;

        public ServiceBase()
        {
            ConfigureLogging();
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            _logger.Info("Loading configuration.");

            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddEnvironmentVariables();

            Configuration = builder.Build();

            _logger.Info("Configuration loaded.");
        }

        private void ConfigureLogging()
        {
            if (!File.Exists(log4netFilename))
                throw new FileNotFoundException($"Failed to find '{log4netFilename}' file.", log4netFilename);

            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead(log4netFilename));

            var repo = LogManager.CreateRepository(
                Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));

            log4net.Config.XmlConfigurator.Configure(repo, log4netConfig[log4netName]);

            _logger.Info("Logging initialized.");
        }
    }
}
