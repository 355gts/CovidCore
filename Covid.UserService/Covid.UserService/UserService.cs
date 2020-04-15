using Autofac;
using Covid.Service.Common;
using Covid.UserService.Container;
using Covid.UserService.EventListeners;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;

namespace Covid.UserService
{
    sealed class UserService : ServiceBase, ServiceControl
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(UserService));

        private readonly CancellationTokenSource _eventListenerCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IList<Task> _tasks = new List<Task>();
        private readonly IContainer _container;

        public UserService()
        {
            _container = ContainerConfiguration.Configure(Configuration, _eventListenerCancellationTokenSource, _cancellationTokenSource);
        }

        public bool Start(HostControl hostControl)
        {
            _logger.Info($"Starting service '{nameof(UserService)}'");

            using (var scope = _container.BeginLifetimeScope())
            {
                var userEventListener = scope.Resolve<UserEventListener>();
                _tasks.Add(userEventListener.Run());
            }

            _logger.Info($"Started service '{nameof(UserService)}'");

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _logger.Info($"Stopping service '{nameof(UserService)}'");
            _eventListenerCancellationTokenSource.Cancel();
            _cancellationTokenSource.Cancel();
            if (_tasks.Any())
            {
                Task.WhenAll(_tasks).GetAwaiter().GetResult();
            }
            _logger.Info($"Stopped service '{nameof(UserService)}'");
            return true;
        }
    }
}
