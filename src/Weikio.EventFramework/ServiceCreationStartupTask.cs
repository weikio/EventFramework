using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.AspNetCore.StartupTasks;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework
{
    public class ServiceCreationStartupTask : IStartupTask
    {
        private readonly ICloudEventGatewayCollection _cloudEventGatewayCollection;
        private readonly ICloudEventRouterServiceFactory _serviceFactory;
        private readonly ILogger<ServiceCreationStartupTask> _logger;

        public ServiceCreationStartupTask(ICloudEventGatewayCollection cloudEventGatewayCollection, ICloudEventRouterServiceFactory serviceFactory, ILogger<ServiceCreationStartupTask> logger)
        {
            _cloudEventGatewayCollection = cloudEventGatewayCollection;
            _serviceFactory = serviceFactory;
            _logger = logger;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting cloud event router services for cloud event gateways.");

            var gateways = _cloudEventGatewayCollection.Gateways.ToList();
            _logger.LogDebug("There's {GatewayCount} gateways.", gateways.Count);

            var gatewaysWithIncomingChannel = gateways.Where(x => x.SupportsIncoming).ToList();
            _logger.LogInformation("There's {GatewayWithIncomingChannelCount} gateways which have incoming channels.", gatewaysWithIncomingChannel.Count);

            var serviceCount = 0;
            foreach (var gateway in gatewaysWithIncomingChannel)
            {
                var incomingChannel = gateway.IncomingChannel;
                var requiredServiceCount = incomingChannel.ReaderCount;
                
                _logger.LogDebug("Starting {Count} services for {Channel}.", requiredServiceCount, incomingChannel);

                for (var i = 0; i < requiredServiceCount; i++)
                {
                    var service = await _serviceFactory.Create(incomingChannel);
                    service.Start(cancellationToken);
                    
                    serviceCount += 1;
                }
            }
            
            _logger.LogInformation("Started {ServiceCount} services.", serviceCount);
        }
    }
}
