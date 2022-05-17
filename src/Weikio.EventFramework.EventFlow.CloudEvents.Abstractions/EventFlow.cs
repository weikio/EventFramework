using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.EventFlow.Abstractions;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class EventFlow
    {
        public EventFlowDefinition FlowDefinition { get; }
        public Type EventSourceType { get; }
        public string Source { get; }

        public EventFlow(EventFlowDefinition flowDefinition, Type eventSourceType, string source)
        {
            FlowDefinition = flowDefinition;
            EventSourceType = eventSourceType;
            Source = source;
        }
        
        public Action<EventSourceInstanceOptions> ConfigureEventSourceInstanceOptions { get; set; }
        public List<ChannelComponent<CloudEvent>> Components { get; set; } = new List<ChannelComponent<CloudEvent>>();
        public List<Func<ComponentFactoryContext, Task<CloudEventsComponent>>> ComponentFactories = new List<Func<ComponentFactoryContext, Task<CloudEventsComponent>>>();
        public List<Endpoint<CloudEvent>> Endpoints { get; set; } = new List<Endpoint<CloudEvent>>();
        public List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)> Interceptors { get; set; } =
            new List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)>();
    }
}
