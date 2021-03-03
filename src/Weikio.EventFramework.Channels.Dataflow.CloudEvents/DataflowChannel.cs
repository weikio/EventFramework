using System;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Channels.Dataflow.CloudEvents
{
    public class DataflowChannel : DataflowChannelBase<object, CloudEvent>
    {
        public DataflowChannel(CloudEventsDataflowChannelOptions options) : base(options)
        {
            Layers.Add(new AdapterLayerBuilder().Build());
            Layers.Add(new ComponentLayerBuilder().Build(options));
        }

        public DataflowChannel(string name = ChannelName.Default, Action<CloudEvent> endpoint = null) : this(
            new CloudEventsDataflowChannelOptions() { Name = name, Endpoint = endpoint })
        {
        }
    }
}
