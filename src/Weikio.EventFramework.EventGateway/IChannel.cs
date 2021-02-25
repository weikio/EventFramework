using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventGateway
{
    public interface IChannel
    {
        string Name { get; }
        Task Send(CloudEvent cloudEvent);
    }
}
