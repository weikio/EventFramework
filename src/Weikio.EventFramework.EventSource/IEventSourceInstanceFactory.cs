using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public interface IEventSourceInstanceFactory
    {
        EventSourceInstance Create(Abstractions.EventSource eventSource,EventSourceInstanceOptions options);
    }
}
