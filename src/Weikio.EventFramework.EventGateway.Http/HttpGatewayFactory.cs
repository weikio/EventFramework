using System;
using System.Net.Http;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventGateway.Http
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

        public ICloudEventGateway Create(string name, string endpoint, string outgoingEndpoint = null, Func<HttpClient> clientFactory = null)
        {
            if (clientFactory == null)
            {
                clientFactory = () => _httpClientFactory.CreateClient(name);
            }

            var result = new HttpGateway(name, endpoint, _initializer.Initialize, outgoingEndpoint, clientFactory);

            return result;
        }
    }
}
