using System;
using System.Collections.Generic;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Channels.Dataflow.CloudEvents;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public class CloudEventsDataflowChannelOptions
    {
        public string Name { get; set; }
        public List<Endpoint> Endpoints { get; set; } = new List<Endpoint>();
        public Action<CloudEvent> Endpoint { get; set; }
        public List<Component> Components { get; set; } = new List<Component>();
        public ILoggerFactory LoggerFactory { get; set; }

        public Func<DataflowLayerGeneric<object, CloudEvent>> AdapterLayerBuilder { get; set; } = () =>
        {
            var b = new AdapterLayerBuilder();

            return b.Build();
        };

        public Func<DataflowChannelOptionsBase<object, CloudEvent>, DataflowLayerGeneric<CloudEvent, CloudEvent>> ComponentLayerBuilder { get; set; } = opt =>
        {
            var b = new ComponentLayerBuilder();

            return b.Build(opt);
        };
    }
}
