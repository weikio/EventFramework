using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventSource.Abstractions
{
    public class EventSourceInstance
    {
        public EventSourceInstanceOptions Options { get; }
        private readonly Func<IServiceProvider, EventSourceInstance, Task<bool>> _start;
        private readonly Func<IServiceProvider, EventSourceInstance, Task<bool>> _stop;
        public string Id { get; }
        public EventSource EventSource { get; }
        public EventSourceStatus Status { get; }
        public TimeSpan? PollingFrequency { get => Options.PollingFrequency; }
        public string CronExpression { get => Options.CronExpression; }
        public MulticastDelegate Configure { get => Options.Configure; }
        public string InternalChannelId { get; set; }
        
        public EventSourceInstance(string id, EventSource eventSource, EventSourceInstanceOptions options, Func<IServiceProvider, EventSourceInstance, Task<bool>> start, 
            Func<IServiceProvider, EventSourceInstance, Task<bool>> stop)
        {
            Options = options;
            _start = start;
            _stop = stop;
            EventSource = eventSource;
            Status = new EventSourceStatus();
            Id = id;
        }
        
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public async Task Start(IServiceProvider serviceProvider)
        {
            await _start(serviceProvider, this);
        }
        
        public async Task Stop(IServiceProvider serviceProvider)
        {
            await _stop(serviceProvider, this);
        }
        
        private sealed class IdEqualityComparer : IEqualityComparer<EventSourceInstance>
        {
            public bool Equals(EventSourceInstance x, EventSourceInstance y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null))
                {
                    return false;
                }

                if (ReferenceEquals(y, null))
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                return x.Id == y.Id;
            }

            public int GetHashCode(EventSourceInstance obj)
            {
                return (obj.Id != null ? obj.Id.GetHashCode() : 0);
            }
        }

        public static IEqualityComparer<EventSourceInstance> IdComparer { get; } = new IdEqualityComparer();

        protected bool Equals(EventSourceInstance other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((EventSourceInstance) obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public static bool operator ==(EventSourceInstance left, EventSourceInstance right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EventSourceInstance left, EventSourceInstance right)
        {
            return !Equals(left, right);
        }
    }
}
