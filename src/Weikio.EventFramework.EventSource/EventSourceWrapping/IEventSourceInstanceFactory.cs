using System;
using Weikio.EventFramework.EventCreator;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public interface IEventSourceInstanceFactory
    {
        EsInstance Create(EventSource eventSource, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, CloudEventCreationOptions cloudEventCreationOptions = null);
    }
}
