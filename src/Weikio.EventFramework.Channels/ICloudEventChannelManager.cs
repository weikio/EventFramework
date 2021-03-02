using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.EventFramework.Channels
{
    public interface ICloudEventChannelManager
    {
        IEnumerable<IChannel> Channels { get; }
        IChannel Get(string channelName);

        void Add(string channelName, IChannel channel);

        Task Update();
    }
}