using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.LongPolling
{
    public class DefaultLongPollingEventSourceHost : BackgroundService, ILongPollingEventSourceHost
    {
        private Func<CancellationToken, IAsyncEnumerable<object>> _eventSource;
        private readonly ICloudEventPublisher _cloudEventPublisher;

        public DefaultLongPollingEventSourceHost(ICloudEventPublisher cloudEventPublisher)
        {
            _cloudEventPublisher = cloudEventPublisher;
        }

        public void Initialize(Func<CancellationToken, IAsyncEnumerable<object>> eventSource)
        {
            _eventSource = eventSource;
        }

        public async Task StartPolling(CancellationToken stoppingToken)
        {
            if (_eventSource == null)
            {
                throw new ArgumentNullException(nameof(_eventSource));
            }
            
            await foreach (var newEvent in _eventSource(stoppingToken))
            {
                await _cloudEventPublisher.Publish(newEvent);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await StartPolling(stoppingToken);
        }
    }
}
