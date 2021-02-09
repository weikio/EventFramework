using System.Threading.Tasks;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;

namespace Weikio.EventFramework.EventGateway.Http
{
    public class HttpGatewayInitializer
    {
        private readonly EndpointManager _endpointManager;
        private readonly IApiProvider _apiProvider;

        public HttpGatewayInitializer(EndpointManager endpointManager, IApiProvider apiProvider)
        {
            _endpointManager = endpointManager;
            _apiProvider = apiProvider;
        }

        public async Task Initialize(HttpGateway gateway)
        {
            var api = _apiProvider.Get(typeof(HttpCloudEventReceiverApi).FullName);
            
            // Create HTTP Endpoint for the gateway
            var endpoint = new Endpoint(gateway.Endpoint, api, new HttpCloudEventReceiverApiConfiguration()
            {
                GatewayName = gateway.Name
            });

            _endpointManager.AddEndpoint(endpoint);
            _endpointManager.Update();
        }
    }
}
