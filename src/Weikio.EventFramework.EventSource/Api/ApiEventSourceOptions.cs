using System;
using Weikio.EventFramework.EventSource.Api.SDK;

namespace Weikio.EventFramework.EventSource.Api
{
    public class ApiEventSourceOptions
    {
        public Func<IApiEventSourceConfiguration, IServiceProvider, string> RouteFunc { get; set; } = (configuration, provider) =>
        {
            if (!string.IsNullOrWhiteSpace(configuration.Route))
            {
                return configuration.Route;
            }

            return "events";
        };
    }
}
