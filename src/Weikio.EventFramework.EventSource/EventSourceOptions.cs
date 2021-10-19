using System;
using Quartz;

namespace Weikio.EventFramework.EventSource
{
    public class EventSourceOptions
    {
        public Func<string, string> EventSourceInstanceChannelNameFactory { get; set; } = instanceId => $"system/eventsourceinstances/{instanceId}";
    }
}
