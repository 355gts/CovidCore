using log4net;
using log4net.Repository;
using System;
using System.Diagnostics;
using System.Net.Http;
using static CommonUtils.Properties.Resources;

namespace CommonUtils.Logging
{
    public sealed class PerformanceLogger : IDisposable
    {
        private const string peformanceLoggerName = "Performance";
        private ILog logger;

        private readonly Stopwatch stopwatch;
        private readonly string methodName;

        private PerformanceLogger(ILoggerRepository loggerRepository)
        {
            if (loggerRepository == null)
                throw new ArgumentNullException(nameof(loggerRepository));

            logger = LogManager.GetLogger(loggerRepository.Name, peformanceLoggerName);

            this.stopwatch = Stopwatch.StartNew();
        }

        public PerformanceLogger(ILoggerRepository loggerRepository, HttpMethod httpMethod, Uri uri)
            : this(loggerRepository)
        {
            if (httpMethod == null)
                throw new ArgumentNullException(nameof(httpMethod));

            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            this.methodName = httpMethod.Method + " " + uri.AbsoluteUri;
        }

        public PerformanceLogger(ILoggerRepository loggerRepository, string methodName)
            : this(loggerRepository)
        {
            this.methodName = methodName;
        }

        public void Dispose()
        {
            stopwatch.Stop();
            logger.InfoFormat(PerformanceLogMessage, this.methodName, this.stopwatch.ElapsedMilliseconds);
        }
    }
}