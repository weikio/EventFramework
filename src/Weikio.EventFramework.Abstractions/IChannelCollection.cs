using System.Collections.Generic;

namespace Weikio.EventFramework.Abstractions
{
    public interface IChannelCollection
    {
        IEnumerable<IChannel> Channels { get; }
        IChannel Get(string channelName);
    }
}