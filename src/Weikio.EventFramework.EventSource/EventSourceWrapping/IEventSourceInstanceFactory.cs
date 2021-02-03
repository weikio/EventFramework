using System;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public interface IEventSourceInstanceFactory
    {
        EsInstance Create(EventSource eventSource, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Action<CloudEventPublisherOptions> configurePublisherOptions = null);
    }
}
