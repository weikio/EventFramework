using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public interface IEventSourceFactory
    {
        EventSource Create(Func<object, Task<(object, object)>> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null);

        EventSource Create(Func<object> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null);

        EventSource Create(object instance, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null);

        EventSource Create<TStateType>(Func<TStateType, (object, TStateType)> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null);

        EventSource Create<TStateType>(Func<TStateType, Task<(object, TStateType)>> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null);

        EventSource Create<TEventSource>(TimeSpan? pollingFrequency = null,
            string cronExpression = null, Action<TEventSource> configure = null);

        EventSource Create(Func<Task<List<object>>> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null);

        EventSource Create(MulticastDelegate action = null, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Type eventSourceType = null, object eventSourceInstance = null);
    }
}
