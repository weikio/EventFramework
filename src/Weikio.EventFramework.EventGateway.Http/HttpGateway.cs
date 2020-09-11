using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventGateway.Http
{
    public class HttpGateway : ICloudEventGateway
    {
        private readonly Func<HttpGateway, Task> _initializer;
        private readonly string _outgoingEndpoint;
        private CancellationToken _cancellationToken;

        public HttpGateway(string name, string endpoint, Func<HttpGateway, Task> initializer = null, string outgoingEndpoint = null, Func<HttpClient> clientFactory = null)
        {
            Status = CloudEventGatewayStatus.New;

            _initializer = initializer;
            _outgoingEndpoint = outgoingEndpoint;
            Name = name;
            Endpoint = endpoint;
            
            var channel = Channel.CreateUnbounded<CloudEvent>();

            IncomingChannel = new IncomingHttpChannel(channel);
            OutgoingChannel = new OutgoingHttpChannel(clientFactory, name, outgoingEndpoint);
        }

        public const string DefaultName = "http";
        public const string DefaultEndpoint = "/api/events";
        public const string DefaultOutgoingEndpoint = "";
        
        public string Name { get; }
        public string Endpoint { get; }
        public IIncomingChannel IncomingChannel { get; }
        public IOutgoingChannel OutgoingChannel { get; }
        public bool SupportsIncoming { get; } = true;
        public bool SupportsOutgoing { get; } = true;
        
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
