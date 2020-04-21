using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Abstractions.DependencyInjection;

namespace Weikio.EventFramework.EventGateway
{
    public static class ServiceCollectionExtensions
    {
        public static IEventFrameworkBuilder AddCloudEventGateway(this IEventFrameworkBuilder builder, Action<CloudEventGatewayOptions> setupAction = null)
        {
            AddCloudEventGateway(builder.Services, setupAction);

            return builder;
        }

        public static IServiceCollection AddCloudEventGateway(this IServiceCollection services, Action<CloudEventGatewayOptions> setupAction = null)
        {
            services.TryAddSingleton<ICloudEventGatewayManager, CloudEventGatewayManager>();
            services.TryAddSingleton<ICloudEventGatewayInitializer, CloudEventGatewayInitializer>();
            
            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return services;
        }
    }
}
