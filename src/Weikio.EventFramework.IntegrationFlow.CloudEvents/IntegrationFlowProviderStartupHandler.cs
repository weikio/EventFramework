using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class IntegrationFlowProviderStartupHandler : IHostedService
    {
        private readonly IntegrationFlowProvider _provider;

        public IntegrationFlowProviderStartupHandler(IntegrationFlowProvider provider)
        {
            _provider = provider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _provider.Initialize(cancellationToken); 
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}