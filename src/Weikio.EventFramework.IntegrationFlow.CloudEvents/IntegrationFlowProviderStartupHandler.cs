using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class IntegrationFlowProviderStartupHandler : IHostedService
    {
        private readonly IntegrationFlowProvider _provider;
        private readonly IntegrationFlowDefinitionProvider _definitionProvider;

        public IntegrationFlowProviderStartupHandler(IntegrationFlowProvider provider, IntegrationFlowDefinitionProvider definitionProvider)
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
