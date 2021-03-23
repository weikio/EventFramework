namespace Weikio.EventFramework.Channels.Dataflow.CloudEvents
{
    public class CloudEventsChannelBuilder : IChannelBuilder
    {
        public IChannel Create(string channelName = ChannelName.Default)
        {
            return new CloudEventsChannel(new CloudEventsDataflowChannelOptions() { Name = channelName });
        }

        public IChannel Create(CloudEventsDataflowChannelOptions options)
        {
            return new CloudEventsChannel(options);
        }
    }
}
