using System;
using System.Net.Http;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.Components.Http
{
    public class HttpEndpointComponent: CloudEventsComponent
    {
        private readonly HttpEndpointOptions _configuration;
        private readonly ILogger<HttpEndpointComponent> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpEndpointComponent(HttpEndpointOptions configuration, ILogger<HttpEndpointComponent> logger, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            Func = Send;
        }

        public async Task<CloudEvent> Send(CloudEvent cloudEvent)
        {
            try
            {
                HttpClient client;

                if (_configuration.ClientFactory != null)
                {
                    client = _configuration.ClientFactory();
                }
                else if (_httpClientFactory != null)
                {
                    client = _httpClientFactory.CreateClient();
                }
                else
                {
                    client = new HttpClient();
                }

                if (_configuration.ConfigureClient != null)
                {
                    await _configuration.ConfigureClient(client);
                }

                var content = new CloudEventContent(cloudEvent,
                    ContentMode.Structured,
                    new JsonEventFormatter());

                var result = await client.PostAsync(_configuration.Endpoint, content);

                if (result.IsSuccessStatusCode == false)
                {
                    throw new Exception("Failed to send event. Http status code: " + result.StatusCode);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send cloud event {CloudEvent} to http endpoint {HttpEndpoint}", cloudEvent, _configuration.Endpoint);
            }

            return cloudEvent;
        }
    }
}