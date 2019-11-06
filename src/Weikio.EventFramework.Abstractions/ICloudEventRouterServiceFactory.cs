using System.Threading.Tasks;

namespace Weikio.EventFramework.Abstractions
{
    public interface ICloudEventRouterServiceFactory
    {
        Task<ICloudEventRouterService> Create(IIncomingChannel channel);
    }
}
