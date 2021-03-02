using System.Threading.Tasks;

namespace Weikio.EventFramework.Channels
{
    public interface IChannel
    {
        string Name { get; }
        Task<bool> Send(object cloudEvent);
        void Subscribe(IChannel channel);
        void Unsubscribe(IChannel channel);
    }
}
