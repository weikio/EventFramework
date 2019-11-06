using System.Collections.Generic;

namespace Weikio.EventFramework.Abstractions
{
    public interface ICloudEventGatewayCollection
    {
        IEnumerable<ICloudEventGateway> Gateways { get; }
        ICloudEventGateway Get(string gatewayName);

        void Add(string gatewayName, ICloudEventGateway gateway);
    }
}
