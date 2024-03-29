﻿using System.Collections.Generic;
using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels.CloudEvents
{
    public interface ICloudEventsChannelManager 
    {
        public void Add(CloudEventsChannel channel);
        public CloudEventsChannel GetDefaultChannel();
        public CloudEventsChannel Get(string channelName);
        public void Remove(CloudEventsChannel channel);
        IEnumerable<IChannel> Channels { get; }
    }
}
