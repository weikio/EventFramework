using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventCreator;

namespace Weikio.EventFramework.EventDefinition
{
    public static class ServiceCollectionExtensions
    {
        public static IEventFrameworkBuilder AddCloudEventDefinitions(this IEventFrameworkBuilder builder, Action<CloudEventCreationOptions> setupAction = null)
        {
            AddCloudEventDefinitions(builder.Services, setupAction);

            return builder;
        }

        public static IServiceCollection AddCloudEventDefinitions(this IServiceCollection services, Action<CloudEventCreationOptions> setupAction = null)
        {
            services.TryAddSingleton<ICloudEventDefinitionManager, DefaultCloudEventDefinitionManager>();
            services.TryAddSingleton<CloudEventToDefinitionConverter>();

            return services;
        }
    }
}
