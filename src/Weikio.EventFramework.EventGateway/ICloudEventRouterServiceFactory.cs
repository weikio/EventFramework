using System.Threading.Tasks;

namespace Weikio.EventFramework.EventGateway
{
    public interface ICloudEventRouterServiceFactory
    {
        Task<ICloudEventRouterService> Create(IIncomingChannel channel, ICloudEventGateway gateway);
    }
}
