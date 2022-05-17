using Weikio.EventFramework.EventSource.Api.SDK;

namespace Weikio.EventFramework.EventSource.Api
{
    public abstract class ApiEventSourceConfigurationBase : IApiEventSourceConfiguration
    {
        public virtual string Route { get; set; }
        public virtual string AuthorizationPolicy { get; set; }
    }

    public class DefaultApiEventSourceConfiguration : ApiEventSourceConfigurationBase
    {
    }
}
