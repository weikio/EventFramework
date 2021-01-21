using System;
using System.Threading;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public class EventSource
    {
        public Guid Id { get; private set; }
        public EventSourceStatus Status { get; }

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
            Status = new EventSourceStatus();
        }

        public void SetCancellationTokenSource(CancellationTokenSource cancellationToken)
        {
            CancellationToken = cancellationToken;
        }

        public CancellationTokenSource CancellationToken { get; private set; }
    }
}
