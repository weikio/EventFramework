using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Abstractions
{
    public interface IOutgoingChannel : IChannel
    {
        Task Send(CloudEvent cloudEvent);
    }
}