using CommonUtils.Logging;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;


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
            LoadConfiguration();
            //ConfigureLogging();
        }

        private void LoadConfiguration()
        {
            try
            {
                _logger.Info("Loading configuration.");

                var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                var currdirectory = Directory.GetCurrentDirectory();

                _logger.Info($"Current working directory - '{currdirectory}'.");

                var builder = new ConfigurationBuilder()
                .SetBasePath(currdirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddEnvironmentVariables();

                Configuration = builder.Build();

                _logger.Info("Configuration loaded.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load configuration, error details - {ex.Message}.", ex);
            }
        }

        private void ConfigureLogging()
        {
            try
            {
                _logger.Info($"Attempting to locate log4net config at location: '{log4netFilename}'.");

                //if (!File.Exists(log4netFilename))
                //    throw new FileNotFoundException($"Failed to find '{log4netFilename}' file.", log4netFilename);

                //XmlDocument log4netConfig = new XmlDocument();
                //log4netConfig.Load(File.OpenRead(log4netFilename));

                var loggerRepository = LogManager.CreateRepository(
                    Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));

                //LogConfiguration.Initialize(loggerRepository);
                //log4net.Config.XmlConfigurator.Configure(repo, log4netConfig[log4netName]);

                _logger.Info("Logging initialized.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load log4net configuration, error details - {ex.Message}.", ex);
            }
        }
    }
}
