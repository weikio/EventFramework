using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.Api.SDK
{
    public abstract class ApiEventSourceConfigurationBase : IApiEventSourceConfiguration
    {
        public virtual string Route { get; set; }
        public virtual string AuthorizationPolicy { get; set; }
    }

    internal class DefaultApiEventSourceConfiguration : ApiEventSourceConfigurationBase
    {
    }
}
