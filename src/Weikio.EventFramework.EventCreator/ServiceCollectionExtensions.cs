using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.EventFramework.Abstractions.DependencyInjection;

namespace Weikio.EventFramework.EventCreator
{
    public static class ServiceCollectionExtensions
    {
        public static IEventFrameworkBuilder AddCloudEventCreator(this IEventFrameworkBuilder builder, Action<CloudEventCreationOptions> setupAction = null)
        {
            AddCloudEventCreator(builder.Services, setupAction);

            return builder;
        }

        public static IServiceCollection AddCloudEventCreator(this IServiceCollection services, Action<CloudEventCreationOptions> setupAction = null)
        {
            services.TryAddSingleton<ICloudEventCreator, CloudEventCreator>();
            services.TryAddSingleton<ICloudEventCreatorOptionsProvider, DefaultCloudEventCreatorOptionsProvider>();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return services;
        }

        public static IServiceCollection ConfigureCloudEvent<TEventType>(this IServiceCollection services, Action<CloudEventCreationOptions> setupAction)
        {
            services.TryAddSingleton<ICloudEventCreator, CloudEventCreator>();

            services.Configure(typeof(TEventType).FullName, setupAction);

            return services;
        }
    }
}
