using System.Threading.Channels;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels;

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
        public Task<bool> Send(object cloudEvent)
        {
            throw new System.NotImplementedException();
        }

        public void Subscribe(IChannel channel)
        {
            throw new System.NotImplementedException();
        }

        public void Unsubscribe(IChannel channel)
        {
            throw new System.NotImplementedException();
        }

        public ChannelWriter<CloudEvent> Writer { get; }
        public ChannelReader<CloudEvent> Reader { get; }
        public int ReaderCount { get; set; }
    }
}
