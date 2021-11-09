using System;
using System.Collections.Generic;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.IntegrationFlow
{
    public abstract class IntegrationFlowBase<TOutput>
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public Action<EventSourceInstanceOptions> ConfigureEventSourceInstanceOptions { get; set; }
        public List<ChannelComponent<TOutput>> Components { get; set; } = new List<ChannelComponent<TOutput>>();
        public List<Endpoint<TOutput>> Endpoints { get; set; } = new List<Endpoint<TOutput>>();
        public List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)> Interceptors { get; set; } =
            new List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)>();
    }
}
