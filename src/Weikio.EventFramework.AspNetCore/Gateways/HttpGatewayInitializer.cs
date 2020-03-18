using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Configuration;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.ApiFramework.Core.Infrastructure;

namespace Weikio.EventFramework.AspNetCore.Gateways
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
            var api = await _apiProvider.Get(typeof(HttpCloudEventReceiverApi).FullName);
            
            // Create HTTP Endpoint for the gateway
            var endpoint = new Endpoint(gateway.Endpoint, api, new HttpCloudEventReceiverApiConfiguration()
            {
                GatewayName = gateway.Name
            });

            _endpointManager.AddEndpoint(endpoint);
            _endpointManager.Update();
        }
    }
    
    public class SyncEndpointInitializer : IEndpointInitializer
    {
        private readonly ILogger<EndpointInitializer> _logger;
        private readonly ApiChangeNotifier _changeNotifier;
        private readonly ApiFrameworkOptions _options;

        public SyncEndpointInitializer(ILogger<EndpointInitializer> logger, ApiChangeNotifier changeNotifier, IOptions<ApiFrameworkOptions> options)
        {
            _logger = logger;
            _changeNotifier = changeNotifier;
            _options = options.Value;
        }

        public void Initialize(List<Endpoint> endpoints, bool force = false)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            if (endpoints?.Any() != true)
            {
                return;
            }

            foreach (var endpoint in endpoints)
            {
                Initialize(endpoint, force).Wait();
            }
            
            if (_options.ChangeNotificationType == ChangeNotificationTypeEnum.Batch)
            {
                _changeNotifier.Notify();
            }
        }

        public async Task Initialize(Endpoint endpoint, bool force = false)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            var endpointStatus = endpoint.Status;

            if (force == false && (endpointStatus.Status == EndpointStatusEnum.Ready || endpointStatus.Status == EndpointStatusEnum.Failed))
            {
                return;
            }

            await endpoint.Initialize();

            if (_options.ChangeNotificationType == ChangeNotificationTypeEnum.Single)
            {
                _changeNotifier.Notify();
            }
        }
    }
}
