namespace Weikio.EventFramework.Channels.CloudEvents
{
    public class DefaultCloudEventsChannelBuilder : ICloudEventsChannelBuilder
    {
        public CloudEventsChannel Create(CloudEventsChannelOptions options)
        {
            return new CloudEventsChannel(options);
        }
    }
}