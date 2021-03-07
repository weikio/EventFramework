using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.EventFramework.Channels
{
    public interface IChannelManager
    {
        IEnumerable<IChannel> Channels { get; }
        IChannel Get(string channelName);
        void Add(IChannel channel);
        IChannel GetDefaultChannel();
    }
}
