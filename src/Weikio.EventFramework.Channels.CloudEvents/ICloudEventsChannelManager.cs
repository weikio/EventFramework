using System.Collections.Generic;
using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels.CloudEvents
{
    public interface ICloudEventsChannelManager 
    {
        public void Add(CloudEventsChannel channel);
        public new CloudEventsChannel GetDefaultChannel();
        public new CloudEventsChannel Get(string channelName);
        public void Remove(CloudEventsChannel channel);
        IEnumerable<IChannel> Channels { get; }
    }
}
