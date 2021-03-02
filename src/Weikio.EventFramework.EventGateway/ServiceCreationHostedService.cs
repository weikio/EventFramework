using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.EventGateway
{
    public class ServiceCreationHostedService : IHostedService
    {
        private readonly IEnumerable<ICloudEventGateway> _cloudEventGateways;
        private readonly ICloudEventGatewayManager _cloudEventGatewayManager;
        private readonly ILogger<ServiceCreationHostedService> _logger;

        public ServiceCreationHostedService(IEnumerable<ICloudEventGateway> cloudEventGateways, ICloudEventGatewayManager cloudEventGatewayManager, 
            ILogger<ServiceCreationHostedService> logger)
        {
            _cloudEventGateways = cloudEventGateways;
            _cloudEventGatewayManager = cloudEventGatewayManager;
            _logger = logger;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating cloud event gateways.");

            foreach (var cloudEventGateway in _cloudEventGateways)
            {
                _cloudEventGatewayManager.Add(cloudEventGateway.Name, cloudEventGateway);
            }
            
            var gateways = _cloudEventGatewayManager.Gateways.ToList();
            _logger.LogDebug("There's {GatewayCount} gateways to initialize.", gateways.Count);

            await _cloudEventGatewayManager.Update();
            
            _logger.LogInformation("Initialized {GatewayCount} gateways.", gateways.Count);
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Execute(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
