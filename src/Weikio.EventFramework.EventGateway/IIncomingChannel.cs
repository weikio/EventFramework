using System.Threading.Channels;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventGateway
{
    public interface IIncomingChannel : IChannel
    {
        ChannelWriter<CloudEvent> Writer { get; }
        ChannelReader<CloudEvent> Reader { get; }
        int ReaderCount { get; set; }
    }
}
