using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels;

namespace Weikio.EventFramework.EventGateway.Gateways.Null
{
    public class NullOutgoingChannel : IOutgoingChannel
    {
        public NullOutgoingChannel(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public Task<bool> Send(object cloudEvent)
        {
            return Task.FromResult(true);
        }

        public void Subscribe(IChannel channel)
        {
            throw new System.NotImplementedException();
        }

        public void Unsubscribe(IChannel channel)
        {
            throw new System.NotImplementedException();
        }
    }
}
