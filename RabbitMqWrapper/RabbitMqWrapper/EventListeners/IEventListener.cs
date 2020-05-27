using System.Threading;

namespace RabbitMqWrapper.EventListeners
{
    public interface IEventListener
    {
        void Run(CancellationToken cancellationToken);
    }
}