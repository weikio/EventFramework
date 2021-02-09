using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventSource
{
    public interface IEventSourceFactory
    {
        EventSourceInstance Create(Func<object, Task<(object, object)>> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null);

        EventSourceInstance Create(Func<object> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null);

        EventSourceInstance Create(object instance, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null);

        EventSourceInstance Create<TStateType>(Func<TStateType, (object, TStateType)> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null);

        EventSourceInstance Create<TStateType>(Func<TStateType, Task<(object, TStateType)>> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null);

        EventSourceInstance Create<TEventSource>(TimeSpan? pollingFrequency = null,
            string cronExpression = null, Action<TEventSource> configure = null);

        EventSourceInstance Create(Func<Task<List<object>>> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null);

        EventSourceInstance Create(MulticastDelegate action = null, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Type eventSourceType = null, object eventSourceInstance = null);

        Abstractions.EventSource Create(string name, Version version, MulticastDelegate action = null, Type eventSourceType = null, object eventSourceInstance = null);
    }
}
