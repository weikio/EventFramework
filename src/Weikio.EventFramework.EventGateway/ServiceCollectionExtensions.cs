using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.EventAggregator.Core;

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
            services.AddCloudEventAggregatorCore();
            services.AddChannels();
            
            services.TryAddSingleton<ICloudEventGatewayManager, CloudEventGatewayManager>();
            services.TryAddSingleton<ICloudEventGatewayInitializer, CloudEventGatewayInitializer>();
            services.AddHostedService<ServiceCreationHostedService>();
            services.TryAddSingleton<ICloudEventRouterServiceFactory, CloudEventRouterServiceFactory>();
            services.TryAddTransient<ICloudEventRouterService, CloudEventRouterService>();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return services;
        }
    }
}
