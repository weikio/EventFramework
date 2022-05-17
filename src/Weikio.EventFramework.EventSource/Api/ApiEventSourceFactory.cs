using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Apis;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Api.SDK;

namespace Weikio.EventFramework.EventSource.Api
{
    internal class ApiEventSourceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ApiEventSourceFactory> _logger;
        private readonly IEndpointManager _endpointManager;
        private readonly IApiProvider _apiProvider;
        private readonly ApiEventSourceOptions _options;
        
        public ApiEventSourceFactory(IServiceProvider serviceProvider, ILogger<ApiEventSourceFactory> logger, IEndpointManager endpointManager,
            IApiProvider apiProvider,
            IOptions<ApiEventSourceOptions> options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _endpointManager = endpointManager;
            _apiProvider = apiProvider;
            _options = options.Value;
        }

        public async Task Create(Type apiEventSourceConfigurationType, Type apiEventSourceType, ICloudEventPublisher cloudEventPublisher,
            IApiEventSourceConfiguration configuration, CancellationToken stoppingToken)
        {
            var catalog = new TypeApiCatalog(typeof(ApiEventSourceWrapperApi));
            await catalog.Initialize(stoppingToken);

            var existingApis = _apiProvider.List();
            var alreadyAdded = false;

            foreach (var newApi in catalog.List())
            {
                if (existingApis.Contains(newApi))
                {
                    alreadyAdded = true;

                    break;
                }
            }

            if (alreadyAdded == false)
            {
                _apiProvider.Add(catalog);
            }

            var apiDefinition = catalog.List().Single();
            var api = _apiProvider.Get(apiDefinition);

            if (configuration == null && apiEventSourceConfigurationType != null)
            {
                configuration = (IApiEventSourceConfiguration)ActivatorUtilities.CreateInstance(_serviceProvider, apiEventSourceConfigurationType);
            }
            else if (configuration == null)
            {
                configuration = new DefaultApiEventSourceConfiguration();
            }

            var config = configuration;
            config.Route = _options.RouteFunc(config, _serviceProvider);

            var apiConfig = new ApiEventSourceWrapperApiConfiguration()
            {
                CloudEventPublisher = cloudEventPublisher,
                ApiType = apiEventSourceType,
                EndpointConfiguration = config,
                ApiConfigurationType = apiEventSourceConfigurationType ?? typeof(DefaultApiEventSourceConfiguration)
            };

            // Create HTTP Endpoint for the gateway
            var endpoint = new Endpoint(apiConfig.EndpointConfiguration.Route, api, apiConfig);
            
            _endpointManager.AddEndpoint(endpoint);
            _endpointManager.Update();

            var tcs = new TaskCompletionSource<bool>();
            stoppingToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            await tcs.Task;

            _endpointManager.RemoveEndpoint(endpoint);
            _endpointManager.Update();
        }
    }
}
