using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.HealthChecks;
using Weikio.EventFramework.EventGateway.Http.ApiFrameworkIntegration;

namespace Weikio.EventFramework.EventGateway.Http
{
    public class HttpGatewayInitializer
    {
        private readonly EndpointManager _endpointManager;
        private readonly IApiProvider _apiProvider;
        private readonly CustomEndpointConfigurationProvider _customEndpointConfigurationProvider;
        private readonly ILogger<HttpGatewayInitializer> _logger;

        public HttpGatewayInitializer(EndpointManager endpointManager, IApiProvider apiProvider, 
            CustomEndpointConfigurationProvider customEndpointConfigurationProvider, ILogger<HttpGatewayInitializer> logger)
        {
            _endpointManager = endpointManager;
            _apiProvider = apiProvider;
            _customEndpointConfigurationProvider = customEndpointConfigurationProvider;
            _logger = logger;
        }

        public Task Initialize(HttpGateway gateway)
        {
            _logger.LogInformation("Initializing http gateway {HttpGateway}", gateway);
            
            if (_apiProvider.IsInitialized)
            {
                var api = _apiProvider.Get(typeof(HttpCloudEventReceiverApi).FullName);
            
                // Create HTTP Endpoint for the gateway
                var endpoint = new Endpoint(gateway.Endpoint, api, new HttpCloudEventReceiverApiConfiguration()
                {
                    GatewayName = gateway.Name
                });

                _logger.LogDebug("Creating and starting the http gateway {HttpGateway} on the fly", gateway);

                _endpointManager.AddEndpoint(endpoint);
                _endpointManager.Update();

                _logger.LogInformation("Created and started http gateway {HttpGateway} on the fly", gateway);
                return Task.CompletedTask;
            }

            _logger.LogDebug("System not fully initialized yet, delaying creation and starting of the http gateway {HttpGateway}", gateway);

            var endpointDefinition = new EndpointDefinition(gateway.Endpoint, typeof(HttpCloudEventReceiverApi).FullName,
                new HttpCloudEventReceiverApiConfiguration() { GatewayName = gateway.Name }, new EmptyHealthCheck(), "gateway");

            _customEndpointConfigurationProvider.Add(endpointDefinition);

            return Task.CompletedTask;
        }
    }
}
