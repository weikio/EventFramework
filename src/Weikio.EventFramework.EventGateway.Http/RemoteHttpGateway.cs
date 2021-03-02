using System;
using System.Threading.Tasks;
using Weikio.EventFramework.Channels;

namespace Weikio.EventFramework.EventGateway.Http
{
    public class RemoteHttpGateway : ICloudEventGateway
    {
        private readonly RemoteHttpGatewayOptions _options;

        public string Name
        {
            get
            {
                return _options.Name;
            }
        }

        public IIncomingChannel IncomingChannel { get; } = null;
        public IOutgoingChannel OutgoingChannel { get; private set; }
        
        public Task Initialize()
        {
            Status = CloudEventGatewayStatus.Ready;
            OutgoingChannel = new OutgoingHttpChannel(_options.ClientFactory, Name, _options.Endpoint, _options.ConfigureClient);

            return Task.CompletedTask;
        }

        public RemoteHttpGateway(RemoteHttpGatewayOptions options)
        {
            _options = options;
        }

        public CloudEventGatewayStatus Status { get; set; }
        public bool SupportsIncoming { get; } = false;
        public bool SupportsOutgoing { get; } = true;
    }
}
