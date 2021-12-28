using System;
using System.Collections.Generic;

namespace Weikio.EventFramework.Channels.Abstractions
{
    public class ComponentFactoryContext
    {
        public IServiceProvider ServiceProvider { get; }
        public int ComponentIndex { get; }
        public string ChannelName { get;  }
        public List<(string Key, object Value)> Tags { get; }

        public ComponentFactoryContext(IServiceProvider serviceProvider, int componentIndex, string channelName, List<(string, object)> tags = null)
        {
            ServiceProvider = serviceProvider;
            ComponentIndex = componentIndex;
            ChannelName = channelName;
            Tags = tags ?? new List<(string, object)>();
        }
    }
}
