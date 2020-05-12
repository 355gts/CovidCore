using CommonUtils.Logging.Configuration;
using CommonUtils.Properties;
using log4net;
using log4net.Config;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using System;
using System.IO;
using System.Reflection;

namespace CommonUtils.Logging
{
    public static class LogConfiguration
    {
        public static void Initialize(ILog4NetConfiguration log4NetConfiguration, string applicationInstallDirectory = null)
        {
            Initialize(LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(Hierarchy)), log4NetConfiguration, applicationInstallDirectory);
        }

        public static void Initialize(ILoggerRepository loggerRepository, ILog4NetConfiguration log4NetConfiguration, string applicationInstallDirectory = null)
        {
            if (loggerRepository == null)
                throw new ArgumentNullException(nameof(loggerRepository));

            if (log4NetConfiguration == null)
                throw new ArgumentNullException(nameof(log4NetConfiguration));

            GlobalContext.Properties["COMPONENT-NAME"] = log4NetConfiguration.ComponentName;

            if (string.IsNullOrWhiteSpace(applicationInstallDirectory)
                || !Directory.Exists(applicationInstallDirectory))
            {
                applicationInstallDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            }

            string log4netConfigPath = Path.Combine(applicationInstallDirectory, log4NetConfiguration.ConfigurationFileName);

            // If we're debugging, and the log4net file doesn't exist in the root folder, check in the 'bin' folder.
            if (!File.Exists(log4netConfigPath))
            {
                log4netConfigPath = Path.Combine(applicationInstallDirectory, "bin", log4NetConfiguration.ConfigurationFileName);
            }

            var logConfigFile = new FileInfo(log4netConfigPath);

            if (!logConfigFile.Exists)
            {
                throw new ArgumentException(string.Format(
                    Resources.LoggingConfigurationFileNotFoundError,
                    log4NetConfiguration.ConfigurationFileName,
                    applicationInstallDirectory));
            }

            XmlConfigurator.ConfigureAndWatch(loggerRepository, logConfigFile);
        }
    }
}
