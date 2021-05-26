using System;

namespace Weikio.EventFramework.EventSource
{
    public class EventSourceOptions
    {
        public Func<string, string> EventSourceInstanceChannelNameFactory { get; set; } = instanceId =>
        {
            return $"system/eventsourceinstances/{instanceId}";
        };
    }
}
