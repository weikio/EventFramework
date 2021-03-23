using System.Collections.Generic;

namespace Weikio.EventFramework.Channels
{
    public interface IChannelCollection
    {
        IEnumerable<IChannel> Channels { get; }
        IChannel Get(string channelName);
    }
}