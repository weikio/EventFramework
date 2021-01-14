using System;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public class EventSource
    {
        public bool IsInitialized { get; set; }
        public Guid Id { get; private set; }

        public MulticastDelegate Action { get; } = null;

        public TimeSpan? PollingFrequency { get; } = null;

        public string CronExpression { get; } = null;

        public MulticastDelegate Configure { get; }

        public Type EventSourceType { get; }

        public object EventSourceInstance { get; } = null;

        public EventSource(Guid id, MulticastDelegate action = null, TimeSpan? pollingFrequency = null, string cronExpression = null,
            MulticastDelegate configure = null, Type eventSourceType = null, object eventSourceInstance = null)
        {
            Action = action;
            PollingFrequency = pollingFrequency;
            CronExpression = cronExpression;
            Configure = configure;
            EventSourceType = eventSourceType;
            EventSourceInstance = eventSourceInstance;
            Id = id;
        }
    }
}
