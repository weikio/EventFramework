using CloudNative.CloudEvents;
using Weikio.EventFramework.EventCreator;

namespace Weikio.EventFramework.Channels.Dataflow.CloudEvents
{
    public class CloudEventsDataflowChannelOptions : DataflowChannelOptionsBase<object, CloudEvent>
    {
        public CloudEventsDataflowChannelOptions()
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
