using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels
{
    public interface IChannelBuilder
    {
        IChannel Create(string channelName = ChannelName.Default);
    }
}
