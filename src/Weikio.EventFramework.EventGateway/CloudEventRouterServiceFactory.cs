using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Channels;

namespace Weikio.EventFramework.EventGateway
{
    public class CloudEventRouterServiceFactory : ICloudEventRouterServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CloudEventRouterServiceFactory> _logger;

        public CloudEventRouterServiceFactory(IServiceProvider serviceProvider, ILogger<CloudEventRouterServiceFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task<ICloudEventRouterService> Create(IIncomingChannel channel, ICloudEventGateway gateway)
        {
            try
            {
                _logger.LogDebug("Creating Event router service for {Channel}", channel);
                
                var result = _serviceProvider.GetService<ICloudEventRouterService>();
                result.Initialize(channel, gateway);

                _logger.LogDebug("Created and initialized event router service for {Channel}", channel);

                return Task.FromResult(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create Event router service for {Channel}", channel);

                throw;
            }
        }
    }
}
