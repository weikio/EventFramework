using System;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public interface IEventSourceInstanceFactory
    {
        EsInstance Create(EventSource eventSource,EventSourceInstanceOptions options);
    }
}
