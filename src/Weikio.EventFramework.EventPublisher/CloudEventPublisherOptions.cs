using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventPublisher
{
    public class CloudEventPublisherOptions
    {
        public string DefaultGatewayName { get; set; } = GatewayName.Default;
    }
}
