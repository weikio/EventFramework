using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Channels.Dataflow.CloudEvents
{
    public class CloudEventsEndpoint : Endpoint<CloudEvent>
    {
        public CloudEventsEndpoint(Func<CloudEvent, Task> func, Predicate<CloudEvent> predicate = null) : base(func, predicate)
        {
        }
        
        public static implicit operator Func<CloudEvent, Task>(CloudEventsEndpoint component)
        {
            return component.Func;
        }
    
        public static implicit operator Predicate<CloudEvent>(CloudEventsEndpoint component)
        {
            return component.Predicate;
        }
    
        public static implicit operator CloudEventsEndpoint(Func<CloudEvent, Task> func)
        {
            return new(func);
        }
    
        public static implicit operator CloudEventsEndpoint((Func<CloudEvent, Task> Func, Predicate<CloudEvent> Predicate) def)
        {
            return new(def.Func, def.Predicate);
        }
    }
    
    public class CloudEventsComponent : DataflowChannelComponent<CloudEvent>
    {
        public CloudEventsComponent(Func<CloudEvent, CloudEvent> func, Predicate<CloudEvent> predicate = null) : base(func, predicate)
        {
        }
        
        public static implicit operator Func<CloudEvent, CloudEvent>(CloudEventsComponent component)
        {
            return component.Func;
        }

        public static implicit operator Predicate<CloudEvent>(CloudEventsComponent component)
        {
            return component.Predicate;
        }

        public static implicit operator CloudEventsComponent(Func<CloudEvent, CloudEvent> func)
        {
            return new(func);
        }

        public static implicit operator CloudEventsComponent((Func<CloudEvent, CloudEvent> Func, Predicate<CloudEvent> Predicate) def)
        {
            return new(def.Func, def.Predicate);
        }
    }
}
