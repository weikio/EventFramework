using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.Api.SDK
{
    public interface IApiEventSourceConfiguration
    {
        string Route { get; set; }
        string AuthorizationPolicy { get; set; }
    }
}
