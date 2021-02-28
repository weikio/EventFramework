using System.Threading.Channels;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventGateway.Http
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
        public Task Send(object cloudEvent)
        {
            throw new System.NotImplementedException();
        }

        public ChannelWriter<CloudEvent> Writer { get; }
        public ChannelReader<CloudEvent> Reader { get; }
        public int ReaderCount { get; set; }
    }
}
