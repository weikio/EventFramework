using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventCreator;

namespace Weikio.EventFramework.EventPublisher
{
    public static class ServiceCollectionExtensions
    {
        public static IEventFrameworkBuilder AddCloudEventPublisher(this IEventFrameworkBuilder builder, Action<CloudEventPublisherOptions> setupAction = null)
        {
            AddCloudEventPublisher(builder.Services, setupAction);

            return builder;
        }

        public static IServiceCollection AddCloudEventPublisher(this IServiceCollection services, Action<CloudEventPublisherOptions> setupAction = null)
        {
            services.TryAddSingleton<ICloudEventPublisher, CloudEventPublisher>();
            services.AddCloudEventCreator();
            
            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return services;
        }
    }
}
