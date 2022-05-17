using System;

namespace Weikio.EventFramework.Channels.CloudEvents
{
    public class DefaultCloudEventChannelOptions
    {
        public Action<CloudEventsChannel> InitAction { get; set; } = (channel) => { };
        public Action<CloudEventsChannel> AfterInitAction { get; set; } = (channel) => { };
    }
}
