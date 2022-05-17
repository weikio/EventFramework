using System.Threading.Channels;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels
{
    public interface IIncomingChannel : IChannel
    {
        ChannelWriter<CloudEvent> Writer { get; }
        ChannelReader<CloudEvent> Reader { get; }
        int ReaderCount { get; set; }
    }
}
