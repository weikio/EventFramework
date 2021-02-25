using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventGateway.Gateways.Null
{
    public class NullOutgoingChannel : IOutgoingChannel
    {
        public NullOutgoingChannel(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public Task Send(CloudEvent cloudEvent)
        {
            return Task.CompletedTask;
        }
    }
    
    public class NullChannel : IChannel
    {
        public string Name { get; }

        public NullChannel(string name)
        {
            Name = name;
        }

        public Task Send(CloudEvent cloudEvent)
        {
            return Task.CompletedTask;
        }
    }
}
