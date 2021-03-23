using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.EventGateway.Http
{
    public class HttpEndpoint
    {
        private readonly RemoteHttpGatewayOptions _configuration;
        private readonly ILogger<HttpEndpoint> _logger;

        public HttpEndpoint(RemoteHttpGatewayOptions configuration, ILogger<HttpEndpoint> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Send(CloudEvent cloudEvent)
        {
            try
            {
                var client = _configuration.ClientFactory();

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
        }
    }
}
