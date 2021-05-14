using System;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.Dataflow;

namespace Weikio.EventFramework.Channels.CloudEvents
{
    public class CloudEventsChannel : DataflowChannelBase<object, CloudEvent>
    {
        public CloudEventsChannel(CloudEventsChannelOptions options) : base(options)
        {
        }

        public CloudEventsChannel(string name = ChannelName.Default, Action<CloudEvent> endpoint = null) : this(
            new CloudEventsChannelOptions() { Name = name, Endpoint = endpoint })
        {
        }

        public override string ToString()
        {
            return $"{Options.Name} (Pubsub: {Options.IsPubSub}";
        }
    }
}
