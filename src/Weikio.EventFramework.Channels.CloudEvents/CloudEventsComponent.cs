using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;

namespace Weikio.EventFramework.Channels.CloudEvents
{
    public class CloudEventsComponent : ChannelComponent<CloudEvent>
    {
        public CloudEventsComponent(Func<CloudEvent, CloudEvent> func, Predicate<CloudEvent> predicate = null) : base(func, predicate)
        {
        }
        
        public static implicit operator Func<CloudEvent, Task<CloudEvent>>(CloudEventsComponent component)
        {
            return component.Func;
        }

        public static implicit operator Predicate<CloudEvent>(CloudEventsComponent component)
        {
            return component.Predicate;
        }

        public static implicit operator CloudEventsComponent(Func<CloudEvent, CloudEvent> func)
        {
            return new CloudEventsComponent(func);
        }

        public static implicit operator CloudEventsComponent((Func<CloudEvent, CloudEvent> Func, Predicate<CloudEvent> Predicate) def)
        {
            return new CloudEventsComponent(def.Func, def.Predicate);
        }
    }
}