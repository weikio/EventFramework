using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels
{
    public interface IChannelManager
    {
        /// <summary>
        /// Get all the channel
        /// </summary>
        IEnumerable<IChannel> Channels { get; }
        
        /// <summary>
        /// Get a channel by channel name
        /// </summary>
        IChannel Get(string channelName);
        
        /// <summary>
        /// Add a channel
        /// </summary>
        void Add(IChannel channel);
        
        /// <summary>
        /// Gets the default channel
        /// </summary>
        IChannel GetDefaultChannel();
        
        /// <summary>
        /// Removes a channel
        /// </summary>
        /// <param name="channel"></param>
        void Remove(IChannel channel);
    }
}
