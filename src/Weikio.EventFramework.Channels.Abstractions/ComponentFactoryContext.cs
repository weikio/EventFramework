using System;
using System.Collections.Generic;

namespace Weikio.EventFramework.Channels.Abstractions
{
    public class ComponentFactoryContext
    {
        public IServiceProvider ServiceProvider { get; }
        public int ComponentIndex { get; }
        public string ComponentChannelName { get;  }
        public List<(string Key, object Value)> Tags { get; }

        public ComponentFactoryContext(IServiceProvider serviceProvider, int componentIndex, string componentChannelName, List<(string, object)> tags = null)
        {
            ServiceProvider = serviceProvider;
            ComponentIndex = componentIndex;
            ComponentChannelName = componentChannelName;
            Tags = tags ?? new List<(string, object)>();
        }
    }
}
