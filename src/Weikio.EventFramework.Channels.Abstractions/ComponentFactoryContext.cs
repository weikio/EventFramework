using System;
using System.Collections.Generic;

namespace Weikio.EventFramework.Channels.Abstractions
{
    public class ComponentFactoryContext
    {
        public IServiceProvider ServiceProvider { get; }
        public int ComponentIndex { get; }
        public string ChannelName { get; }
        public List<(string Key, object Value)> Tags { get; }

        public ComponentFactoryContext(IServiceProvider serviceProvider, int componentIndex, string channelName, List<(string, object)> tags = null)
        {
            ServiceProvider = serviceProvider;
            ComponentIndex = componentIndex;
            ChannelName = channelName;
            Tags = tags ?? new List<(string, object)>();
        }
    }

    public class Step
    {
        public string Current { get; set; }
        public List<string> Nexts { get; set; } = new List<string>();

        public Step(string current, string next) : this(current, new List<string>() { next })
        {
        }

        public Step(string current, List<string> nexts)
        {
            Current = current;
            Nexts.AddRange(nexts);
        }

        public void Next(string name)
        {
            Nexts.Add(name);
        }
    }
}
