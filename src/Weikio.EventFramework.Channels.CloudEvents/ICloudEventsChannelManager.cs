namespace Weikio.EventFramework.Channels.CloudEvents
{
    public interface ICloudEventsChannelManager 
    {
        public void Add(CloudEventsChannel channel);
        public new CloudEventsChannel GetDefaultChannel();
        public new CloudEventsChannel Get(string channelName);
        public void Remove(CloudEventsChannel channel);
    }
}