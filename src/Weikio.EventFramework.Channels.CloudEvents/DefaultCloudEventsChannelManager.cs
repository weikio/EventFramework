using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels.CloudEvents
{
    public class DefaultCloudEventsChannelManager : ICloudEventsChannelManager
    {
        private readonly IChannelManager _channelManager;
        private readonly IOptions<DefaultCloudEventChannelOptions> _options;

        public DefaultCloudEventsChannelManager(IChannelManager channelManager, IOptions<DefaultCloudEventChannelOptions> options)
        {
            _channelManager = channelManager;
            _options = options;
        }

        public void Add(CloudEventsChannel channel)
        {
            var optionsValue = _options.Value;
            optionsValue.InitAction?.Invoke(channel);
            
            _channelManager.Add(channel);
            optionsValue.AfterInitAction?.Invoke(channel);
        }

        public CloudEventsChannel GetDefaultChannel()
        {
            return (CloudEventsChannel) _channelManager.GetDefaultChannel();
        }

        public CloudEventsChannel Get(string channelName)
        {
            return (CloudEventsChannel) _channelManager.Get(channelName);
        }

        public void Remove(CloudEventsChannel channel)
        {
            _channelManager.Remove(channel);
        }

        public IEnumerable<IChannel> Channels
        {
            get
            {
                return _channelManager.Channels.Where(x => x is CloudEventsChannel);
            }
        }
    }
}
