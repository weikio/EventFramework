using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.CloudEvents.Abstractions;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.EventCreator;

namespace Weikio.EventFramework.Channels.CloudEvents
{
    public class CloudEventsChannelOptions : DataflowChannelOptionsBase<object, CloudEvent>
    {
        public CloudEventsChannelOptions()
        {
            AdapterLayerBuilder = options =>
            {
                var b = new AdapterLayerBuilder();
        
                return b.Build(this);
            };
            
            ComponentLayerBuilder = options =>
            {
                var b = new ComponentLayerBuilder();
        
                return b.Build(this);
            };
        }

        public CloudEventCreationOptions CloudEventCreationOptions { get; set; } = new CloudEventCreationOptions();
    }
}
