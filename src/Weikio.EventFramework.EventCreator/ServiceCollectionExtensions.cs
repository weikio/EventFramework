using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.EventFramework.Abstractions.DependencyInjection;

namespace Weikio.EventFramework.EventCreator
{
    public static class ServiceCollectionExtensions
    {
        public static IEventFrameworkBuilder AddEventCreator(this IEventFrameworkBuilder builder, Action<EventCreationOptions> setupAction = null)
        {
            AddEventCreator(builder.Services, setupAction);

            return builder;
        }

        public static IServiceCollection AddEventCreator(this IServiceCollection services, Action<EventCreationOptions> setupAction = null)
        {
            services.TryAddSingleton<ICloudEventCreator, CloudEventCreator>();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return services;
        }

        public static IServiceCollection ConfigureCloudEvent<TEventType>(this IServiceCollection services, Action<EventCreationOptions> setupAction)
        {
            services.TryAddSingleton<ICloudEventCreator, CloudEventCreator>();

            services.Configure(typeof(TEventType).FullName, setupAction);

            return services;
        }
    }
}
