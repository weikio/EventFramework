using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.ApiFramework;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.EventFramework.Abstractions.DependencyInjection;

namespace Weikio.EventFramework.EventSource.Api
{
    public static class EventFrameworkApiEventSourcesExtensions
    {
        public static IEventFrameworkBuilder AddApiEventSources(this IEventFrameworkBuilder builder)
        {
            AddApiEventSources(builder.Services);

            return builder;
        }

        public static IServiceCollection AddApiEventSources(this IServiceCollection services)
        {
            services.AddHealthChecks();
            services.AddHttpContextAccessor();
            services.AddHttpClient();
            services.TryAddSingleton<ApiEventSourceFactory>();

            if (services.All(x => x.ServiceType != typeof(IEndpointInitializer)))
            {
                services.AddApiFrameworkCore();
            }

            return services;
        }
    }
}
