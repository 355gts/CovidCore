using Autofac;
using Covid.Service.Common;
using Covid.$ext_safeprojectname$.Container;
using Covid.$ext_safeprojectname$.EventListeners;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;

namespace Covid.$ext_safeprojectname$
{
    sealed class $ext_safeprojectname$Service : ServiceBase, ServiceControl
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof($ext_safeprojectname$Service));

        private readonly CancellationTokenSource _eventListenerCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IList<Task> _tasks = new List<Task>();
        private readonly IContainer _container;

        public $ext_safeprojectname$Service()
        {
            _container = ContainerConfiguration.Configure(Configuration, _eventListenerCancellationTokenSource, _cancellationTokenSource);
        }

        public bool Start(HostControl hostControl)
        {
            _logger.Info($"Starting service '{nameof($ext_safeprojectname$Service)}'");

            using (var scope = _container.BeginLifetimeScope())
            {
                var serviceEventListener = scope.Resolve<ServiceEventListener>();
                _tasks.Add(serviceEventListener.Run());
            }

            _logger.Info($"Started service '{nameof($ext_safeprojectname$Service)}'");

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _logger.Info($"Stopping service '{nameof($ext_safeprojectname$Service)}'");
            _eventListenerCancellationTokenSource.Cancel();
            _cancellationTokenSource.Cancel();
            if (_tasks.Any())
            {
                Task.WhenAll(_tasks).GetAwaiter().GetResult();
            }
            _logger.Info($"Stopped service '{nameof($ext_safeprojectname$Service)}'");
            return true;
        }
    }
}
