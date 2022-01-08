using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.ApiFramework;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventSource.Api.SDK.ApiFrameworkIntegration;

namespace Weikio.EventFramework.EventSource.Api.SDK
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
                // TODO: Collection concurrent problem in Api Framework
                services.TryAddSingleton<IEndpointInitializer, SyncEndpointInitializer>();
services.TryAddSingleton<IApiProvider>();
                services.AddApiFrameworkCore(options =>
                {
                    options.AutoResolveEndpoints = false;
                    options.EndpointHttpVerbResolver = new CustomHttpVerbResolver();
                });
                
            }
            
            

            return services;
        }
    }
}
