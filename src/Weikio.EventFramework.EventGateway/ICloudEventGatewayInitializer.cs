using System.Threading.Tasks;

namespace Weikio.EventFramework.EventGateway
{
    public interface ICloudEventGatewayInitializer
    {
        Task Initialize(ICloudEventGateway gateway);
    }
}
