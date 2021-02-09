using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class DefaultEventSourceFactory : IEventSourceFactory
    {
        public EventSourceInstance Create(Func<object, Task<(object, object)>> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            return Create(action, pollingFrequency, cronExpression, configure, null, null);
        }

        public EventSourceInstance Create(Func<object> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            return Create(action, pollingFrequency, cronExpression, configure, null, null);
        }

        public EventSourceInstance Create(object instance, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            return Create(null, pollingFrequency, cronExpression, configure, null, instance);
        }

        public EventSourceInstance Create<TStateType>(Func<TStateType, (object, TStateType)> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            Func<TStateType, Task<(object, TStateType)>> taskAction = state => Task.FromResult(action(state));

            return Create(taskAction, pollingFrequency, cronExpression, configure, null, null);
        }

        public EventSourceInstance Create<TStateType>(Func<TStateType, Task<(object, TStateType)>> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            return Create(action, pollingFrequency, cronExpression, configure, null, null);
        }

        public EventSourceInstance Create<TEventSource>(TimeSpan? pollingFrequency = null,
            string cronExpression = null, Action<TEventSource> configure = null)
        {
            return Create(null, pollingFrequency, cronExpression, configure, typeof(TEventSource), null);
        }

        public EventSourceInstance Create(Func<Task<List<object>>> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            return Create(action, pollingFrequency, cronExpression, configure, null, null);
        }

        public EventSourceInstance Create(MulticastDelegate action = null, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Type eventSourceType = null, object eventSourceInstance = null)
        {
            var id = Guid.NewGuid();

            return new EventSourceInstance(id, action, pollingFrequency, cronExpression, configure, eventSourceType, eventSourceInstance);
        }
        
        public Abstractions.EventSource Create(string name, Version version, MulticastDelegate action = null, Type eventSourceType = null, object eventSourceInstance = null)
        {
            var definition = new EventSourceDefinition(name, version);

            var result = new Abstractions.EventSource(definition, action, eventSourceType, eventSourceInstance);

            return result;
        }

    }
}
