using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class IntegrationFlowInstanceOptions
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public Action<EventSourceInstanceOptions> ConfigureEventSourceInstanceOptions { get; set; }
        public List<ChannelComponent<CloudEvent>> Components { get; set; } = new List<ChannelComponent<CloudEvent>>();
        public List<Func<ComponentFactoryContext, Task<CloudEventsComponent>>> ComponentFactories = new List<Func<ComponentFactoryContext, Task<CloudEventsComponent>>>();
        public List<Endpoint<CloudEvent>> Endpoints { get; set; } = new List<Endpoint<CloudEvent>>();
        public List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)> Interceptors { get; set; } =
            new List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)>();
        public object Configuration { get; set; }
        public IntegrationFlowDefinition FlowDefinition { get; }
    }
}
