using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventGateway.Http
{
    public class HttpCloudEventReceiverApiConfiguration
    {
        public string GatewayName { get; set; }
        public string TargetChannelName { get; set; }
        public string PolicyName { get; set; }
        
        public ICloudEventPublisher CloudEventPublisher { get; set; }
    }
}
