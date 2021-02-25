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
    
    public interface ICloudEventChannelManager
    {
        IEnumerable<IChannel> Channels { get; }
        IChannel Get(string channelName);

        void Add(string channelName, IChannel channel);

        Task Update();
    }
}
