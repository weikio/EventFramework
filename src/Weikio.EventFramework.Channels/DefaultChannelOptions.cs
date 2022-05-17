using System;
using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels
{
    public class DefaultChannelOptions
    {
        public string DefaultChannelName { get; set; } = ChannelName.Default;
        public Action<IChannel> InitAction { get; set; } = (channel) => { };
    }
}
