using System;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventSource.EventSourceWrapping;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.EventSource
{
    public class EsInstance
    {
        private readonly Func<IServiceProvider, EsInstance, Task<bool>> _start;
        private readonly Func<IServiceProvider, EsInstance, Task<bool>> _stop;
        public Guid Id { get; }
        public EventSource EventSource { get; }
        public EventSourceStatus Status { get; }
        public TimeSpan? PollingFrequency { get; }

        public string CronExpression { get; }

        public MulticastDelegate Configure { get; }

        public EsInstance(EventSource eventSource, TimeSpan? pollingFrequency, string cronExpression, MulticastDelegate configure, Func<IServiceProvider, EsInstance, Task<bool>> start, 
            Func<IServiceProvider, EsInstance, Task<bool>> stop)
        {
            _start = start;
            _stop = stop;
            EventSource = eventSource;
            PollingFrequency = pollingFrequency;
            CronExpression = cronExpression;
            Configure = configure;
            Status = new EventSourceStatus();
            Id = Guid.NewGuid();
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
    }
    
    public static class CloudEventExtensions 
    {
        public static Guid? EventSourceId(this CloudEvent cloudEvent)
        {
            if (cloudEvent?.GetAttributes()?.ContainsKey(EventFrameworkEventSourceExtension.EventFrameworkEventSourceAttributeName) == true)
            {
                return (Guid) cloudEvent.GetAttributes()[EventFrameworkEventSourceExtension.EventFrameworkEventSourceAttributeName];
            }

            return null;
        }
    }
}
