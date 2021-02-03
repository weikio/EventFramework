using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public class EventSourceProviderStartupHandler : IHostedService
    {
        private readonly EventSourceProvider _eventSourceProvider;

        public EventSourceProviderStartupHandler(EventSourceProvider eventSourceProvider)
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