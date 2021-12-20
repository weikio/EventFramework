using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class EventFlowProviderStartupHandler : IHostedService
    {
        private readonly EventFlowProvider _provider;
        private readonly EventFlowDefinitionProvider _definitionProvider;

        public EventFlowProviderStartupHandler(EventFlowProvider provider, EventFlowDefinitionProvider definitionProvider)
        {
            _provider = provider;
            _definitionProvider = definitionProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _definitionProvider.Initialize(cancellationToken);
            await _provider.Initialize(cancellationToken); 
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
