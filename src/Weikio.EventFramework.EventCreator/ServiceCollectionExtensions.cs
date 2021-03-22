using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
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
            services.TryAddSingleton<ICloudEventCreator>(provider =>
            {
                var logger = provider.GetService<ILogger<CloudEventCreator>>();
                var optionsProvider = provider.GetService<ICloudEventCreatorOptionsProvider>();
                
                return new CloudEventCreator(logger, optionsProvider, provider);
            });
            
            services.TryAddSingleton<ICloudEventCreatorOptionsProvider, DefaultCloudEventCreatorOptionsProvider>();
            services.TryAddSingleton<ICloudEventDefinitionManager, DefaultCloudEventDefinitionManager>();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return services;
        }

        public static IServiceCollection ConfigureCloudEvent<TEventType>(this IServiceCollection services, Action<CloudEventCreationOptions> setupAction)
        {
            services = services.AddCloudEventCreator(setupAction);

            services.Configure(typeof(TEventType).FullName, setupAction);

            return services;
        }
    }
}
