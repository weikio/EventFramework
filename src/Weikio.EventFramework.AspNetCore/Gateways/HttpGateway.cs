using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.EventFramework.Abstractions;

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
    
    public class HttpGateway : ICloudEventGateway
    {
        private readonly Func<HttpGateway, Task> _initializer;
        private CancellationToken _cancellationToken;

        public HttpGateway(string name, string endpoint, Func<HttpGateway, Task> initializer)
        {
            Status = CloudEventGatewayStatus.New;

            _initializer = initializer;
            Name = name;
            Endpoint = endpoint;
            
            var channel = Channel.CreateUnbounded<CloudEvent>();

            IncomingChannel = new IncomingHttpChannel(channel);
            OutgoingChannel = null;
        }

        public string Name { get; }
        public string Endpoint { get; }
        public IIncomingChannel IncomingChannel { get; }
        public IOutgoingChannel OutgoingChannel { get; }
        public bool SupportsIncoming { get; }
        public bool SupportsOutgoing { get; }
        
        public async Task Initialize()
        {
            if (_initializer != null)
            {
                await _initializer(this);
            }
            
            var cancelToken = new CancellationTokenSource();

            _cancellationToken = new CancellationToken();
            
            Status = CloudEventGatewayStatus.Ready;
        }

        public CloudEventGatewayStatus Status { get; set; }

        public void Dispose()
        {
            
        }
    }
}
