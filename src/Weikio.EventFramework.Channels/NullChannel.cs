using System.Threading.Tasks;
using Weikio.EventFramework.Channels.Abstractions;

namespace Weikio.EventFramework.Channels
{
    public class NullChannel : IChannel
    {
        public string Name { get; }

        public NullChannel(string name)
        {
            Name = name;
        }

        public Task<bool> Send(object cloudEvent)
        {
            return Task.FromResult(true);
        }

        public void Subscribe(IChannel channel)
        {
            throw new System.NotImplementedException();
        }

        public void Unsubscribe(IChannel channel)
        {
            throw new System.NotImplementedException();
        }
    }
}
