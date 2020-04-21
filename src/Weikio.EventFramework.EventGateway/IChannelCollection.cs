using System.Collections.Generic;

namespace Weikio.EventFramework.EventGateway
{
    public interface IChannelCollection
    {
        IEnumerable<IChannel> Channels { get; }
        IChannel Get(string channelName);
    }
}