using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class EventSourceProviderStartupHandler : IHostedService
    {
        private readonly IEventSourceProvider _eventSourceProvider;

        public EventSourceProviderStartupHandler(IEventSourceProvider eventSourceProvider)
        {
            _eventSourceProvider = eventSourceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _eventSourceProvider.Initialize(cancellationToken); 
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
