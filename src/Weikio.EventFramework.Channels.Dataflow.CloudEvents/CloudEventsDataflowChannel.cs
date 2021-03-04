using System;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Channels.Dataflow.CloudEvents
{
    public class CloudEventsDataflowChannel : DataflowChannelBase<object, CloudEvent>
    {
        public CloudEventsDataflowChannel(CloudEventsDataflowChannelOptions options) : base(options)
        {
        }

        public CloudEventsDataflowChannel(string name = ChannelName.Default, Action<CloudEvent> endpoint = null) : this(
            new CloudEventsDataflowChannelOptions() { Name = name, Endpoint = endpoint })
        {
        }
    }
}
