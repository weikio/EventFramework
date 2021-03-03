using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public class Endpoint
    {
        public Func<CloudEvent, Task> Func { get; private set; }
        public Predicate<CloudEvent> Predicate { get; private set; }

        public Endpoint(Func<CloudEvent, Task> func, Predicate<CloudEvent> predicate = null)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            Func = func;
            Predicate = predicate ?? (ev => true);
        }

        public static implicit operator Func<CloudEvent, Task>(Endpoint component)
        {
            return component.Func;
        }

        public static implicit operator Predicate<CloudEvent>(Endpoint component)
        {
            return component.Predicate;
        }

        public static implicit operator Endpoint(Func<CloudEvent, Task> func)
        {
            return new Endpoint(func);
        }

        public static implicit operator Endpoint((Func<CloudEvent, Task> Func, Predicate<CloudEvent> Predicate) def)
        {
            return new Endpoint(def.Func, def.Predicate);
        }
    }
}
