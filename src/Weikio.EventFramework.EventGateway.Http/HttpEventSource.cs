using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventGateway.Http.ApiFrameworkIntegration;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventGateway.Http
{
    public class HttpEventSource : BackgroundService
    {
        private readonly ILogger<HttpEventSource> _logger;
        private readonly EndpointManager _endpointManager;
        private readonly IApiProvider _apiProvider;
        private readonly ICloudEventPublisher _cloudEventPublisher;
        private readonly HttpEventSourceConfiguration _configuration;

        public HttpEventSource(ILogger<HttpEventSource> logger, EndpointManager endpointManager, IApiProvider apiProvider,
            ICloudEventPublisher cloudEventPublisher, HttpEventSourceConfiguration configuration = null)
        {
            _logger = logger;
            _endpointManager = endpointManager;
            _apiProvider = apiProvider;
            _cloudEventPublisher = cloudEventPublisher;

            if (configuration == null)
            {
                configuration = new HttpEventSourceConfiguration();
            }

            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Initializing http event source with configuration {Configuration}", _configuration);

            var api = _apiProvider.Get(typeof(HttpCloudEventReceiverApi).FullName);

            // Create HTTP Endpoint for the gateway
            var endpoint = new Endpoint(_configuration.Endpoint, api,
                new HttpCloudEventReceiverApiConfiguration()
                {
                    PolicyName = _configuration.PolicyName, CloudEventPublisher = _cloudEventPublisher, 
                });

            _endpointManager.AddEndpoint(endpoint);
            _endpointManager.Update();

            var tcs = new TaskCompletionSource<bool>();
            stoppingToken.Register(s => ((TaskCompletionSource<bool>) s).SetResult(true), tcs);
            await tcs.Task;

            _endpointManager.RemoveEndpoint(endpoint);
            _endpointManager.Update();
        }
    }
}
