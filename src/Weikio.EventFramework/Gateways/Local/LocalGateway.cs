using System.Threading.Channels;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Gateways
{
    public class LocalGateway : ICloudEventGateway
    {
        public LocalGateway(string name = GatewayName.Default)
        {
            Name = name;
            var channel = Channel.CreateUnbounded<CloudEvent>();

            IncomingChannel = new LocalIncomingChannel(name + ChannelName.IncomingPostFix, channel, channel);
            OutgoingChannel = new LocalOutgoingChannel(name + ChannelName.OutgoingPostFix, channel);
        }

        public string Name { get; }
        public IIncomingChannel IncomingChannel { get; }
        public IOutgoingChannel OutgoingChannel { get; }
        public bool SupportsIncoming => true;
        public bool SupportsOutgoing => true;
        public Task Initialize()
        {
            Status = CloudEventGatewayStatus.Ready;
            return Task.CompletedTask;
        }

        public CloudEventGatewayStatus Status { get; set; }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}
