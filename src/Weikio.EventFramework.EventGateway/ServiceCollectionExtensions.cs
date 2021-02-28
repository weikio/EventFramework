using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Abstractions.DependencyInjection;
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
            
            services.TryAddSingleton<ICloudEventGatewayManager, CloudEventGatewayManager>();
            services.TryAddSingleton<ICloudEventChannelManager, DefaultCloudEventChannelManager>();
            services.TryAddSingleton<ICloudEventGatewayInitializer, CloudEventGatewayInitializer>();
            services.AddHostedService<ServiceCreationHostedService>();
            services.TryAddSingleton<ICloudEventRouterServiceFactory, CloudEventRouterServiceFactory>();
            services.TryAddTransient<ICloudEventRouterService, CloudEventRouterService>();
            services.TryAddSingleton<IChannelBuilder, DefaultChannelBuilder>();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return services;
        }
    }
}
