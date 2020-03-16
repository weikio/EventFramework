using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Abstractions
{
    public interface ICloudEventPublisher
    {
        Task<CloudEvent> Publish(CloudEvent cloudEvent, string gatewayName = GatewayName.Default);
    }
}
