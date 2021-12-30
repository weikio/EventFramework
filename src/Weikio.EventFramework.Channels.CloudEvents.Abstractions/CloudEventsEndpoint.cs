using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;

namespace Weikio.EventFramework.Channels.CloudEvents
{
    public class CloudEventsEndpoint : Endpoint<CloudEvent>
    {
        protected CloudEventsEndpoint() { }

        public CloudEventsEndpoint(Action<CloudEvent> func, Predicate<CloudEvent> predicate = null) : base(func, predicate)
        {
        }
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
            return new CloudEventsEndpoint(func);
        }

        public static implicit operator CloudEventsEndpoint((Func<CloudEvent, Task> Func, Predicate<CloudEvent> Predicate) def)
        {
            return new CloudEventsEndpoint(def.Func, def.Predicate);
        }
        
        public static implicit operator CloudEventsEndpoint(CloudEventsComponent component)
        {
            return new CloudEventsEndpoint(component.Func, component.Predicate);
        }
    }
}
