using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Channels.Dataflow.CloudEvents
{
    public class ComponentLayerBuilder
    {
        public DataflowLayerGeneric<CloudEvent, CloudEvent> Build(DataflowChannelOptionsBase<object, CloudEvent> options)
        {
            var builder = new SequentialLayerBuilder<CloudEvent>();

            return builder.Build(options.Components);
        }
    }
}
