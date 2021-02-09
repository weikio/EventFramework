using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public interface IEventSourceInstanceFactory
    {
        EsInstance Create(Abstractions.EventSource eventSource,EventSourceInstanceOptions options);
    }
}
