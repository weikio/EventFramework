using System.Threading.Channels;
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

            IncomingChannel = new LocalIncomingChannel(name + ChannelName.IncomingPostFix, channel);
            OutgoingChannel = new LocalOutgoingChannel(name + ChannelName.OutgoingPostFix, channel);
        }

        public string Name { get; }
        public IIncomingChannel IncomingChannel { get; }
        public IOutgoingChannel OutgoingChannel { get; }
    }
}
