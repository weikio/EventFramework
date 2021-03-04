using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public class DataflowChannelOptionsBase<TInput, TOutput>
    {
        public string Name { get; set; }
        public List<Endpoint<TOutput>> Endpoints { get; set; } = new();
        public Action<TOutput> Endpoint { get; set; }
        public List<DataflowChannelComponent<TOutput>> Components { get; set; } = new();
        public ILoggerFactory LoggerFactory { get; set; }
        public Func<DataflowChannelOptionsBase<TInput, TOutput>, DataflowLayerGeneric<TInput, TOutput>> AdapterLayerBuilder { get; set; } 
        public Func<DataflowChannelOptionsBase<TInput, TOutput>, DataflowLayerGeneric<TOutput, TOutput>> ComponentLayerBuilder { get; set; }
        public bool IsPubSub { get; set; } = true;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(180);
    }
}
