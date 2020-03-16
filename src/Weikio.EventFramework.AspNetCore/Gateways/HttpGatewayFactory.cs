using System.Net.Http;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.AspNetCore.Gateways
{
    public class HttpGatewayFactory
    {
        private readonly HttpGatewayInitializer _initializer;
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpGatewayFactory(HttpGatewayInitializer initializer, IHttpClientFactory httpClientFactory)
        {
            _initializer = initializer;
            _httpClientFactory = httpClientFactory;
        }

        public ICloudEventGateway Create(string name, string endpoint, string outgoingEndpoint = null)
        {
            var result = new HttpGateway(name, endpoint, _initializer.Initialize, outgoingEndpoint, _httpClientFactory);

            return result;
        }
    }
}
