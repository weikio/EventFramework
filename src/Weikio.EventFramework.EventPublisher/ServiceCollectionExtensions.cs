using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventCreator;

namespace Weikio.EventFramework.EventPublisher
{
    public static class ServiceCollectionExtensions
    {
        public static IEventFrameworkBuilder AddEventPublisher(this IEventFrameworkBuilder builder, Action<CloudEventPublisherOptions> setupAction = null)
        {
            AddEventPublisher(builder.Services, setupAction);

            return builder;
        }

        public static IServiceCollection AddEventPublisher(this IServiceCollection services, Action<CloudEventPublisherOptions> setupAction = null)
        {
            services.TryAddSingleton<ICloudEventPublisher, CloudEventPublisher>();
            services.AddEventCreator();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return services;
        }
    }
}
