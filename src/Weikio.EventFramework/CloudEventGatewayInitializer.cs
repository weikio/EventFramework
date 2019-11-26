using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework
{
    public class CloudEventGatewayInitializer : ICloudEventGatewayInitializer
    {
        private readonly ICloudEventRouterServiceFactory _serviceFactory;
        private readonly ILogger<CloudEventGatewayInitializer> _logger;

        public CloudEventGatewayInitializer(ICloudEventRouterServiceFactory serviceFactory, 
            ILogger<CloudEventGatewayInitializer> logger)
        {
            _serviceFactory = serviceFactory;
            _logger = logger;
        }
        
        public async Task Initialize(ICloudEventGateway gateway)
        {
            if (gateway == null)
            {
                throw new ArgumentNullException(nameof(gateway));
            }
            
            _logger.LogInformation("Initializing {Gateway}.", gateway);

            await gateway.Initialize();
            
            if (gateway.SupportsIncoming)
            {
                var incomingChannel = gateway.IncomingChannel;
                var requiredServiceCount = incomingChannel.ReaderCount;
                
                _logger.LogDebug("Starting {Count} services for {IncomingChannel}.", requiredServiceCount, incomingChannel);

                for (var i = 0; i < requiredServiceCount; i++)
                {
                    var service = await _serviceFactory.Create(incomingChannel);
                    service.Start(new CancellationToken());
                }

            }
            
            _logger.LogInformation("{Gateway} initialized.", gateway);
        }
    }
}
