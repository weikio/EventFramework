using System;
using System.Collections.Generic;
using System.Linq;

namespace Weikio.EventFramework.Channels.Abstractions
{
    public class ComponentFactoryContext
    {
        public IServiceProvider ServiceProvider { get; }
        public int ComponentIndex { get; }
        public string ChannelName { get; }
        
        private List<(string Key, object Value)> _tags = new List<(string Key, object Value)>();

        public ILookup<string, (string Key, object Value)> Tags
        {
            get
            {
                return _tags.ToLookup(x => x.Key);
            }
        }

        public ComponentFactoryContext(IServiceProvider serviceProvider, int componentIndex, string channelName, List<(string, object)> tags = null)
        {
            ServiceProvider = serviceProvider;
            ComponentIndex = componentIndex;
            ChannelName = channelName;
            _tags = tags ?? new List<(string, object)>();
        }
    }
}
