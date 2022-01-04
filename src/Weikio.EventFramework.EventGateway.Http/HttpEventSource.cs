using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Apis;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventGateway.Http
{
    public abstract class ApiEventSource : BackgroundService
    {
        private readonly ILogger<ApiEventSource> _logger;
        private readonly IEndpointManager _endpointManager;
        private readonly IApiProvider _apiProvider;
        private readonly ICloudEventPublisher _cloudEventPublisher;
        private readonly HttpEventSourceConfiguration _configuration;

        protected abstract Type ApiEventSourceType { get; }
        
        public ApiEventSource(ILogger<ApiEventSource> logger, IEndpointManager endpointManager, IApiProvider apiProvider,
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
            _logger.LogInformation("Initializing API event source with configuration {Configuration}", _configuration);

            var catalog = new TypeApiCatalog(ApiEventSourceType);
            await catalog.Initialize(stoppingToken);
            
            _apiProvider.Add(catalog);
            
            var apiDefinition = catalog.List().Single();
            var api = _apiProvider.Get(apiDefinition);

            // Create HTTP Endpoint for the gateway
            var endpoint = new Endpoint("/mytest", api, new PublisherConfig()
            {
                CloudEventPublisher = _cloudEventPublisher
            });
            //
            // var endpoint = new Endpoint(_configuration.Endpoint, api,
            //     new HttpCloudEventReceiverApiConfiguration()
            //     {
            //         PolicyName = _configuration.PolicyName, CloudEventPublisher = _cloudEventPublisher, 
            //     });

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
