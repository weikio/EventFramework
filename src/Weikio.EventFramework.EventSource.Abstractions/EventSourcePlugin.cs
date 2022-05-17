using System;

namespace Weikio.EventFramework.EventSource.Abstractions
{
    public class EventSourcePlugin
    {
        public Type EventSourceType { get; set; }
        public Action<EventSourceInstanceOptions> ConfigureInstance { get; set; }
    }
}
