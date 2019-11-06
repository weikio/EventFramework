using System.Threading.Channels;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Abstractions
{
    public interface IIncomingChannel : IChannel
    {
        ChannelReader<CloudEvent> Reader { get; }
        int ReaderCount { get; set; }
    }
}