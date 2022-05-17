using System;

namespace Weikio.EventFramework.EventSource.Abstractions
{
    public class EventSource
    {
        public EventSourceDefinition EventSourceDefinition { get; }
        public MulticastDelegate Action { get; }
        public Type EventSourceType { get; }
        public object Instance { get; }

        public EventSource(EventSourceDefinition eventSourceDefinition, MulticastDelegate action = null, Type eventSourceType = null, object instance = null)
        {
            EventSourceDefinition = eventSourceDefinition;
            Action = action;
            EventSourceType = eventSourceType;
            Instance = instance;
        }
    }
}
