using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.Dataflow;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;

namespace Weikio.EventFramework.Channels.CloudEvents.Abstractions
{
    public class ComponentLayerBuilder
    {
        public DataflowLayerGeneric<CloudEvent, CloudEvent> Build(DataflowChannelOptionsBase<object, CloudEvent> options)
        {
            var builder = new SequentialLayerBuilder<CloudEvent>();

            return builder.Build(options.Components, options.Interceptors);
        }
    }
}
