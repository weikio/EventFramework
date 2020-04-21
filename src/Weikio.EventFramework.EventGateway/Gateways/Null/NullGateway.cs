using System.Threading.Tasks;

namespace Weikio.EventFramework.EventGateway.Gateways.Null
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
        public Task Initialize()
        {
            throw new System.NotImplementedException();
        }

        public CloudEventGatewayStatus Status { get; }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}
