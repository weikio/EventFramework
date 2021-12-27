using System;

namespace Weikio.EventFramework.Channels.Abstractions
{
    public class ComponentFactoryContext
    {
        public IServiceProvider ServiceProvider { get; }
        public int ComponentIndex { get; }
        public string ComponentChannelName { get; set; }

        public ComponentFactoryContext(IServiceProvider serviceProvider, int componentIndex, string componentChannelName)
        {
            ServiceProvider = serviceProvider;
            ComponentIndex = componentIndex;
            ComponentChannelName = componentChannelName;
        }
    }
}
