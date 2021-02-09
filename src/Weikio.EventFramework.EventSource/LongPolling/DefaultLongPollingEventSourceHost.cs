using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.EventSourceWrapping;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.EventSource.LongPolling
{
    public class DefaultLongPollingEventSourceHost : BackgroundService, ILongPollingEventSourceHost
    {
        private readonly ICloudEventPublisherFactory _cloudEventPublisherFactory;
        private Func<CancellationToken, IAsyncEnumerable<object>> _poller;
        private EsInstance _eventSourceInstance;
        private CancellationTokenSource _cancellationTokenSource;
        private CloudEventPublisher _cloudEventPublisher;

        public DefaultLongPollingEventSourceHost(ICloudEventPublisherFactory cloudEventPublisherFactory)
        {
            _cloudEventPublisherFactory = cloudEventPublisherFactory;
        }

        public void Initialize(EsInstance eventSourceInstance, Func<CancellationToken, IAsyncEnumerable<object>> pollers,
            CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _eventSourceInstance = eventSourceInstance;
            _poller = pollers;
            _cloudEventPublisher = _cloudEventPublisherFactory.CreatePublisher(_eventSourceInstance.Id.ToString());
        }

        public Task StartPolling(CancellationToken stoppingToken)
        {
            StartAsync(stoppingToken);

            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _eventSourceInstance.Status.UpdateStatus(EventSourceStatusEnum.Started, "Started long polling");
            
            if (_poller == null)
            {
                throw new ArgumentNullException(nameof(_poller));
            }
            
            await foreach (var newEvent in _poller(_cancellationTokenSource.Token))
            {
                await _cloudEventPublisher.Publish(newEvent);
            }
            
            _eventSourceInstance.Status.UpdateStatus(EventSourceStatusEnum.Stopped, "Stopped long polling");
        }
    }
}
