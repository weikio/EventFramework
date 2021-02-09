using System;
using System.Threading;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class EventSourceInstance
    {
        public Guid Id { get; private set; }
        public EventSourceStatus Status { get; }

        public MulticastDelegate Action { get; } = null;

        public TimeSpan? PollingFrequency { get; } = null;

        public string CronExpression { get; } = null;

        public MulticastDelegate Configure { get; }

        public Type EventSourceType { get; }

        public object Instance { get; } = null;

        public EventSourceInstance(Guid id, MulticastDelegate action = null, TimeSpan? pollingFrequency = null, string cronExpression = null,
            MulticastDelegate configure = null, Type eventSourceType = null, object instance = null)
        {
            Action = action;
            PollingFrequency = pollingFrequency;
            CronExpression = cronExpression;
            Configure = configure;
            EventSourceType = eventSourceType;
            Instance = instance;
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
