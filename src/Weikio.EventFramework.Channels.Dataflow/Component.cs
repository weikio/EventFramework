using System;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public class Component
    {
        public Func<CloudEvent, CloudEvent> Func { get; private set; }
        public Predicate<CloudEvent> Predicate { get; private set; }

        public Component(Func<CloudEvent, CloudEvent> func, Predicate<CloudEvent> predicate = null)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            Func = func;
            Predicate = predicate ?? (ev => true);
        }

        public static implicit operator Func<CloudEvent, CloudEvent>(Component component)
        {
            return component.Func;
        }

        public static implicit operator Predicate<CloudEvent>(Component component)
        {
            return component.Predicate;
        }

        public static implicit operator Component(Func<CloudEvent, CloudEvent> func)
        {
            return new Component(func);
        }

        public static implicit operator Component((Func<CloudEvent, CloudEvent> Func, Predicate<CloudEvent> Predicate) def)
        {
            return new Component(def.Func, def.Predicate);
        }
    }
}
