using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.Api.SDK
{
    public interface IApiEventSourceConfiguration
    {
        ICloudEventPublisher CloudEventPublisher { get; set; }
    }
}
