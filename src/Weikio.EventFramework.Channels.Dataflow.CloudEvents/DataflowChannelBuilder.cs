namespace Weikio.EventFramework.Channels.Dataflow.CloudEvents
{
    public class CloudEventsDataflowChannelBuilder : IChannelBuilder
    {
        public IChannel Create(string channelName = ChannelName.Default)
        {
            return new CloudEventsDataflowChannel(new CloudEventsDataflowChannelOptions() { Name = channelName });
        }

        public IChannel Create(CloudEventsDataflowChannelOptions options)
        {
            return new CloudEventsDataflowChannel(options);
        }
    }
}
