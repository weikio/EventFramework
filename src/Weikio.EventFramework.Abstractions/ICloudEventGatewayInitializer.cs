using System.Threading.Tasks;

namespace Weikio.EventFramework.Abstractions
{
    public interface ICloudEventGatewayInitializer
    {
        Task Initialize(ICloudEventGateway gateway);
    }
}
