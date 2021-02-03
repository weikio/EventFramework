using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public interface IEventSourceInstanceManager
    {
        List<EsInstance> GetAll();
        Guid Create(EventSourceInstanceOptions options);

        Guid Create(string name, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Action<CloudEventPublisherOptions> configurePublisherOptions = null);

        Guid Create(string name, Version version, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Action<CloudEventPublisherOptions> configurePublisherOptions = null);

        Guid Create(EventSourceDefinition eventSourceDefinition, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Action<CloudEventPublisherOptions> configurePublisherOptions = null);

        Guid Create(EventSource eventSource, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Action<CloudEventPublisherOptions> configurePublisherOptions = null);

        Task Start(Guid eventSourceInstanceId);
        Task StartAll();
        Task Stop(Guid eventSourceId);
        Task StopAll();
        Task Remove(Guid eventSourceId);
        Task RemoveAll();
    }
}
