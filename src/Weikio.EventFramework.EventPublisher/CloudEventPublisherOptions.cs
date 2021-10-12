using Weikio.EventFramework.Channels;
using Weikio.EventFramework.EventGateway;

namespace Weikio.EventFramework.EventPublisher
{
    public class CloudEventPublisherOptions
    {
        public string DefaultGatewayName { get; set; } = GatewayName.Default;
        public string DefaultChannelName { get; set; } = ChannelName.Default;
    }
}
