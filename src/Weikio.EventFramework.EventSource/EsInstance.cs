﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventSource.EventSourceWrapping;

namespace Weikio.EventFramework.EventSource
{
    public class EsInstance
    {
        public EventSourceInstanceOptions Options { get; }
        private readonly Func<IServiceProvider, EsInstance, Task<bool>> _start;
        private readonly Func<IServiceProvider, EsInstance, Task<bool>> _stop;
        public Guid Id { get; }
        public EventSource EventSource { get; }
        public EventSourceStatus Status { get; }
        public TimeSpan? PollingFrequency { get => Options.PollingFrequency; }
        public string CronExpression { get => Options.CronExpression; }
        public MulticastDelegate Configure { get => Options.Configure; }
        public EsInstance(Guid id, EventSource eventSource, EventSourceInstanceOptions options, Func<IServiceProvider, EsInstance, Task<bool>> start, 
            Func<IServiceProvider, EsInstance, Task<bool>> stop)
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
    }
}
