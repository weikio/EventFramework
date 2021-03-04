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
