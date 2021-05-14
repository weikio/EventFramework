using System.Collections.Generic;
using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels
{
    public interface IChannelCollection
    {
        IEnumerable<IChannel> Channels { get; }
        IChannel Get(string channelName);
    }
}
