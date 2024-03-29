﻿using System;
using System.Collections.Generic;
using System.Linq;
using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels
{
    public class ChannelCollection : List<IChannel>, IChannelCollection
    {
        public IEnumerable<IChannel> Channels => this;
        
        public IChannel Get(string channelName)
        {
            var result = this.FirstOrDefault(x => string.Equals(channelName, x.Name, StringComparison.InvariantCultureIgnoreCase));

            if (result == null)
            {
                throw new UnknownChannelException(channelName);
            }

            return result;
        }
    }
}
