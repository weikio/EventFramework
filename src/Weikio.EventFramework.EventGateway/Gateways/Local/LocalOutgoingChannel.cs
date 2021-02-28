using System.Threading.Channels;
using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventGateway.Gateways.Local
{
    public class LocalOutgoingChannel : IOutgoingChannel
    {
        private readonly ChannelWriter<CloudEvent> _writer;

        public LocalOutgoingChannel(string name, ChannelWriter<CloudEvent> writer)
        {
            _writer = writer;
            Name = name;
        }

        public string Name { get; }

        public async Task Send(object cloudEvent)
        {
            await _writer.WriteAsync((CloudEvent) cloudEvent);
        }
    }
}
