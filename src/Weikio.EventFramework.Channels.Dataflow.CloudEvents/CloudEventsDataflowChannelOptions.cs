using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Channels.Dataflow.CloudEvents
{
    public class CloudEventsDataflowChannelOptions : DataflowChannelOptionsBase<object, CloudEvent>
    {
        public CloudEventsDataflowChannelOptions()
        {
            AdapterLayerBuilder = () =>
            {
                var b = new AdapterLayerBuilder();
        
                return b.Build();
            };
            
            ComponentLayerBuilder = options =>
            {
                var b = new ComponentLayerBuilder();
        
                return b.Build(options);
            };
        }
    }
}
