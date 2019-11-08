using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Gateways
{
    public class CloudEventGateway : ICloudEventGateway
    {
        public CloudEventGateway(string name, IIncomingChannel incomingChannel, IOutgoingChannel outgoingChannel)
        {
            Name = name;
            IncomingChannel = incomingChannel;
            OutgoingChannel = outgoingChannel;
        }

        public string Name { get; }
        public IIncomingChannel IncomingChannel { get; }
        public IOutgoingChannel OutgoingChannel { get; }
    }
}
