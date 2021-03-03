using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public class DataflowChannelOptionsBase<TInput, TOutput>
    {
        public string Name { get; set; }
        public List<Endpoint> Endpoints { get; set; } = new();
        public Action<TOutput> Endpoint { get; set; }
        public List<Component> Components { get; set; } = new();
        public ILoggerFactory LoggerFactory { get; set; }

        public Func<DataflowLayerGeneric<TInput, TOutput>> AdapterLayerBuilder { get; set; } 
        public Func<DataflowChannelOptionsBase<TInput, TOutput>, DataflowLayerGeneric<TOutput, TOutput>> ComponentLayerBuilder { get; set; }

    }
}
