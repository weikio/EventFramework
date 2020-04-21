using System.Threading.Channels;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventGateway.Gateways.Local
{
    public class LocalIncomingChannel : IIncomingChannel
    {
        public LocalIncomingChannel(string name, ChannelReader<CloudEvent> reader, ChannelWriter<CloudEvent> writer, int? readerCount = 1)
        {
            Name = name;
            Reader = reader;
            Writer = writer;
            ReaderCount = readerCount.GetValueOrDefault();
        }

        public string Name { get; }
        public ChannelWriter<CloudEvent> Writer { get; }
        public ChannelReader<CloudEvent> Reader { get; }
        public int ReaderCount { get; set; }
    }
}
