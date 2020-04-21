using System.Threading;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventGateway
{
    public interface ICloudEventRouterService
    {
        void Initialize(IIncomingChannel incomingChannel, ICloudEventGateway gateway);
        Task Start(CancellationToken cancellationToken);
    }
}
