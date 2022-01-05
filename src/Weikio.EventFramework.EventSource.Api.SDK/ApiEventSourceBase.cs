using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Apis;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Api.SDK;

namespace Weikio.EventFramework.EventGateway.Http
{
    public abstract class ApiEventSourceBase : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ApiEventSourceBase> _logger;
        private readonly IEndpointManager _endpointManager;
        private readonly IApiProvider _apiProvider;
        private readonly ICloudEventPublisher _cloudEventPublisher;
        private object _configuration;
        protected abstract Type ApiEventSourceType { get; }
        protected abstract Type ApiEventSourceConfigurationType { get; }

        public ApiEventSourceBase(IServiceProvider serviceProvider, ILogger<ApiEventSourceBase> logger, IEndpointManager endpointManager, IApiProvider apiProvider,
            ICloudEventPublisher cloudEventPublisher, object configuration = null)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _endpointManager = endpointManager;
            _apiProvider = apiProvider;
            _cloudEventPublisher = cloudEventPublisher;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Initializing API event source with configuration {Configuration}", _configuration);

            var catalog = new TypeApiCatalog(ApiEventSourceType);
            await catalog.Initialize(stoppingToken);

            foreach (var VARIABLE in catalog.List())
            {
                
            }
            _apiProvider.Add(catalog);
            
            var apiDefinition = catalog.List().Single();
            var api = _apiProvider.Get(apiDefinition);

            if (_configuration == null && ApiEventSourceConfigurationType != null)
            {
                _configuration = ActivatorUtilities.CreateInstance(_serviceProvider, ApiEventSourceConfigurationType);
            }
            
            if (_configuration != null)
            {
                ((IApiEventSourceConfiguration)_configuration).CloudEventPublisher = _cloudEventPublisher;
            }
            
            // Create HTTP Endpoint for the gateway
            var endpoint = new Endpoint("/mytest", api, _configuration);
            
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
