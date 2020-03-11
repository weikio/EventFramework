using System.Threading.Channels;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Gateways
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
