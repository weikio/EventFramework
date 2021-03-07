using System;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Channels.Dataflow.CloudEvents
{
    public class CloudEventsChannel : DataflowChannelBase<object, CloudEvent>
    {
        public CloudEventsChannel(CloudEventsDataflowChannelOptions options) : base(options)
        {
        }

        public CloudEventsChannel(string name = ChannelName.Default, Action<CloudEvent> endpoint = null) : this(
            new CloudEventsDataflowChannelOptions() { Name = name, Endpoint = endpoint })
        {
        }

        public override string ToString()
        {
            return $"{Options.Name} (Pubsub: {Options.IsPubSub}";
        }
    }
}
