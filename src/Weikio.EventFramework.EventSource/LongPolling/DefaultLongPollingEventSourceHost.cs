using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Weikio.AspNetCore.StartupTasks;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.EventSourceWrapping;

namespace Weikio.EventFramework.EventSource.LongPolling
{
    public class DefaultLongPollingEventSourceHost : BackgroundService, ILongPollingEventSourceHost
    {
        private Func<CancellationToken, IAsyncEnumerable<object>> _pollers;
        private EventSourceWrapping.EventSource _eventSource;
        private readonly ICloudEventPublisher _cloudEventPublisher;

        public DefaultLongPollingEventSourceHost(ICloudEventPublisher cloudEventPublisher)
        {
            _cloudEventPublisher = cloudEventPublisher;
        }

        public void Initialize(EventSourceWrapping.EventSource eventSource, Func<CancellationToken, IAsyncEnumerable<object>> pollers)
        {
            _eventSource = eventSource;
            _pollers = pollers;
        }

        public Task StartPolling(CancellationToken stoppingToken)
        {
            StartAsync(stoppingToken);

            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _eventSource.Status.UpdateStatus(EventSourceStatusEnum.Running, "Running");
            
            if (_pollers == null)
            {
                throw new ArgumentNullException(nameof(_pollers));
            }
            
            await foreach (var newEvent in _pollers(_eventSource.CancellationToken.Token))
            {
                await _cloudEventPublisher.Publish(newEvent);
            }
            
            _eventSource.Status.UpdateStatus(EventSourceStatusEnum.Stopped, "Stopped");
        }
    }
}
