using System;

namespace Weikio.EventFramework.Channels.CloudEvents
{
    public class ChannelInstanceOptions
    {
        public string Name { get; set; }
        public Action<IServiceProvider, CloudEventsChannelOptions> Configure { get; set; }
    }
}