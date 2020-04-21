using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventGateway
{
    public interface ICloudEventGatewayManager
    {
        IEnumerable<ICloudEventGateway> Gateways { get; }
        ICloudEventGateway Get(string gatewayName);

        void Add(string gatewayName, ICloudEventGateway gateway);

        Task Update();
    }
}
