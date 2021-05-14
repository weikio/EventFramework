using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels
{
    public interface IChannelManager
    {
        IEnumerable<IChannel> Channels { get; }
        IChannel Get(string channelName);
        void Add(IChannel channel);
        IChannel GetDefaultChannel();
        void Remove(IChannel channel);
    }
}
