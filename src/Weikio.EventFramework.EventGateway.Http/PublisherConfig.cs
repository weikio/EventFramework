using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventGateway.Http
{
    public class PublisherConfig
    {
        public ICloudEventPublisher CloudEventPublisher { get; set; }
    }
}
