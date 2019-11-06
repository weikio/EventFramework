using System.Threading.Channels;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Gateways
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

        public async Task Send(CloudEvent cloudEvent)
        {
            await _writer.WriteAsync(cloudEvent);
        }
    }
}