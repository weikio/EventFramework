using System.Threading.Channels;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.AspNetCore.Gateways
{
    public class IncomingHttpChannel : IIncomingChannel
    {
        public IncomingHttpChannel(Channel<CloudEvent> channel)
        {
            Writer = channel.Writer;
            Reader = channel.Reader;
            ReaderCount = 1;
        }

        public string Name { get; }
        public ChannelWriter<CloudEvent> Writer { get; }
        public ChannelReader<CloudEvent> Reader { get; }
        public int ReaderCount { get; set; }
    }
}
