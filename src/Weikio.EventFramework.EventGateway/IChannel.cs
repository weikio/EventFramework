using System.Threading.Channels;
using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventGateway
{
    public interface IChannel
    {
        string Name { get; }
        Task<bool> Send(object cloudEvent);
        void Subscribe(IChannel channel);
        void Unsubscribe(IChannel channel);
    }
}
