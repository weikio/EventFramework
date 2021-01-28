using System.Threading;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public interface IEventSourceInitializer
    {
        EventSourceStatusEnum Initialize(EventSourceInstance eventSourceInstance);
    }
}
