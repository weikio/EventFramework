using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels.Dataflow.Abstractions
{
    public class DataflowChannelOptionsBase<TInput, TOutput>
    {
        public ILoggerFactory LoggerFactory { get; set; }
        public Func<DataflowChannelOptionsBase<TInput, TOutput>, DataflowLayerGeneric<TInput, TOutput>> AdapterLayerBuilder { get; set; }
        public Func<DataflowChannelOptionsBase<TInput, TOutput>, DataflowLayerGeneric<TOutput, TOutput>> ComponentLayerBuilder { get; set; }
        public bool IsPubSub { get; set; } = true;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(180);
        public string Name { get; set; }
        public List<Endpoint<TOutput>> Endpoints { get; set; } = new List<Endpoint<TOutput>>();
        public Action<TOutput> Endpoint { get; set; }
        public List<ChannelComponent<TOutput>> Components { get; set; } = new List<ChannelComponent<TOutput>>();

        public List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)> Interceptors { get; set; } =
            new List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)>();
    }
}
