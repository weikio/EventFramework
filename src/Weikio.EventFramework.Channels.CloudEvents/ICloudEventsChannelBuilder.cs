namespace Weikio.EventFramework.Channels.CloudEvents
{
    public interface ICloudEventsChannelBuilder
    {
        CloudEventsChannel Create(CloudEventsChannelOptions options);
    }
}