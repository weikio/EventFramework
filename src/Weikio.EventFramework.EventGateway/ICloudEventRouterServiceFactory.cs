using System.Threading.Tasks;
using Weikio.EventFramework.Channels;

namespace Weikio.EventFramework.EventGateway
{
    public interface ICloudEventRouterServiceFactory
    {
        Task<ICloudEventRouterService> Create(IIncomingChannel channel, ICloudEventGateway gateway);
    }
}
