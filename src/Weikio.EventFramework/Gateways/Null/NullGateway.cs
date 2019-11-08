using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Gateways.Null
{
    public class NullGateway : ICloudEventGateway
    {
        public NullGateway(string name)
        {
            Name = name;
            OutgoingChannel = new NullOutgoingChannel(name + ChannelName.OutgoingPostFix);
        }

        public string Name { get; }
        public IIncomingChannel IncomingChannel => null;
        public IOutgoingChannel OutgoingChannel { get; }
    }
}
