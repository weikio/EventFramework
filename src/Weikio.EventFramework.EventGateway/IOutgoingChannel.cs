using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventGateway
{
    public interface IOutgoingChannel : IChannel
    {
        Task Send(CloudEvent cloudEvent);
    }
}