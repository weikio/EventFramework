using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.Api.SDK
{
    public abstract class ApiEventSourceConfigurationBase : IApiEventSourceConfiguration
    {
        public string Route { get; set; }
        public ICloudEventPublisher CloudEventPublisher { get; set; }
    }
}
