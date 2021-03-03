using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public class DataflowChannelOptions
    {
        public string Name { get; set; }
        public List<Endpoint> Endpoints { get; set; } = new List<Endpoint>();
        public Action<CloudEvent> Endpoint { get; set; }
        public List<Component> Components { get; set; } = new List<Component>();
        public ILoggerFactory LoggerFactory { get; set; }
    }

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
