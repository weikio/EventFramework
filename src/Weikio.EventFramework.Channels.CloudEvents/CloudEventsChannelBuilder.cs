using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels.CloudEvents
{
    public class CloudEventsChannelBuilder : IChannelBuilder
    {
        public IChannel Create(string channelName = ChannelName.Default)
        {
            return new CloudEventsChannel(new CloudEventsChannelOptions() { Name = channelName });
        }

        public IChannel Create(CloudEventsChannelOptions options)
        {
            return new CloudEventsChannel(options);
        }
    }
}
