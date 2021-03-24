﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Security;

namespace Weikio.EventFramework.Channels.Dataflow.CloudEvents
{
    public interface ICloudEventsChannelManager 
    {
        public void Add(CloudEventsChannel channel);
        public new CloudEventsChannel GetDefaultChannel();
        public new CloudEventsChannel Get(string channelName);
        public void Remove(CloudEventsChannel channel);
    }
    
    public class DefaultCloudEventsChannelManager : ICloudEventsChannelManager
    {
        private readonly IChannelManager _channelManager;

        public DefaultCloudEventsChannelManager(IChannelManager channelManager)
        {
            _channelManager = channelManager;
        }

        public void Add(CloudEventsChannel channel)
        {
            _channelManager.Add(channel);
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