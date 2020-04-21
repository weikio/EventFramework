using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventGateway;

namespace Weikio.EventFramework.EventPublisher
{
    public class CloudEventPublisherOptions
    {
        public string DefaultGatewayName { get; set; } = GatewayName.Default;
    }
}
