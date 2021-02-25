using System.Threading.Channels;
using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventGateway.Gateways.Local
{
    public class LocalGateway : ICloudEventGateway
    {
        public LocalGateway(string name = DefaultName)
        {
            Name = name;
            var channel = Channel.CreateUnbounded<CloudEvent>();

            IncomingChannel = new LocalIncomingChannel(name + ChannelName.IncomingPostFix, channel, channel);
            OutgoingChannel = new LocalOutgoingChannel(name + ChannelName.OutgoingPostFix, channel);
        }

        public const string DefaultName = "local";

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
    }
}
