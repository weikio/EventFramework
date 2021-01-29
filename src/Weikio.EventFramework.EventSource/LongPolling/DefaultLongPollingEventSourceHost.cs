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
        private EventSourceWrapping.EventSourceInstance _eventSourceInstance;
        private readonly ICloudEventPublisher _cloudEventPublisher;

        public DefaultLongPollingEventSourceHost(ICloudEventPublisher cloudEventPublisher)
        {
            _cloudEventPublisher = cloudEventPublisher;
        }

        public void Initialize(EventSourceWrapping.EventSourceInstance eventSourceInstance, Func<CancellationToken, IAsyncEnumerable<object>> pollers)
        {
            _eventSourceInstance = eventSourceInstance;
            _pollers = pollers;
        }

        public Task StartPolling(CancellationToken stoppingToken)
        {
            StartAsync(stoppingToken);

            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // _eventSourceInstance.Status.UpdateStatus(EventSourceStatusEnum.Started, "Running");
            
            if (_pollers == null)
            {
                throw new ArgumentNullException(nameof(_pollers));
            }
            
            await foreach (var newEvent in _pollers(_eventSourceInstance.CancellationToken.Token))
            {
                await _cloudEventPublisher.Publish(newEvent);
            }
            
            // _eventSourceInstance.Status.UpdateStatus(EventSourceStatusEnum.Stopped, "Stopped");
        }
    }
}
