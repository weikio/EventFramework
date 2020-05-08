using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource
{
    public class ContinuousEventSourceHost : BackgroundService
    {
        private readonly Func<CancellationToken, IAsyncEnumerable<object>> _eventSource;
        private readonly ICloudEventPublisher _cloudEventPublisher;

        public ContinuousEventSourceHost(Func<CancellationToken, IAsyncEnumerable<object>> eventSource, ICloudEventPublisher cloudEventPublisher)
        {
            _eventSource = eventSource;
            _cloudEventPublisher = cloudEventPublisher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var newEvent in _eventSource(stoppingToken))
            {
                await _cloudEventPublisher.Publish(newEvent);
            }
        }
    }
}
