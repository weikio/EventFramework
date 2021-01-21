using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public class DefaultEventSourceFactory : IEventSourceFactory
    {
        public EventSource Create(Func<object, Task<(object, object)>> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            return Create(action, pollingFrequency, cronExpression, configure, null, null);
        }

        public EventSource Create(Func<object> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            return Create(action, pollingFrequency, cronExpression, configure, null, null);
        }

        public EventSource Create(object instance, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            return Create(null, pollingFrequency, cronExpression, configure, null, instance);
        }

        public EventSource Create<TStateType>(Func<TStateType, (object, TStateType)> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            Func<TStateType, Task<(object, TStateType)>> taskAction = state => Task.FromResult(action(state));

            return Create(taskAction, pollingFrequency, cronExpression, configure, null, null);
        }

        public EventSource Create<TStateType>(Func<TStateType, Task<(object, TStateType)>> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            return Create(action, pollingFrequency, cronExpression, configure, null, null);
        }

        public EventSource Create<TEventSource>(TimeSpan? pollingFrequency = null,
            string cronExpression = null, Action<TEventSource> configure = null)
        {
            return Create(null, pollingFrequency, cronExpression, configure, typeof(TEventSource), null);
        }

        public EventSource Create(Func<Task<List<object>>> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            return Create(action, pollingFrequency, cronExpression, configure, null, null);
        }

        public EventSource Create(MulticastDelegate action = null, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Type eventSourceType = null, object eventSourceInstance = null)
        {
            var id = Guid.NewGuid();

            return new EventSource(id, action, pollingFrequency, cronExpression, configure, eventSourceType, eventSourceInstance);
        }
    }
}
