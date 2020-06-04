using System.Threading;

namespace RabbitMQWrapper.EventListeners
{
    public interface IEventListener
    {
        void Run(CancellationToken cancellationToken);
    }
}