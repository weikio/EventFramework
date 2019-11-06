using System.Threading;
using System.Threading.Tasks;

namespace Weikio.EventFramework.Abstractions
{
    public interface ICloudEventRouterService
    {
        void Initialize(IIncomingChannel incomingChannel);
        Task Start(CancellationToken cancellationToken);
    }
}
