using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.EventFramework.Channels.Abstractions
{
    public interface IChannel
    {
        string Name { get; }
        Task<bool> Send(object cloudEvent);
        List<(string Key, object Value)> Tags => new List<(string, object)>();
    }
}
