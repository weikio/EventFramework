using System;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventAggregator.Core;

namespace Weikio.EventFramework.EventAggregator.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IEventFrameworkBuilder AddCloudEventAggregator(this IEventFrameworkBuilder builder, Action<CloudEventAggregatorOptions> setupAction = null)
        {
            AddCloudEventAggregator(builder.Services, setupAction);

            return builder;
        }

        public static IServiceCollection AddCloudEventAggregator(this IServiceCollection services, Action<CloudEventAggregatorOptions> setupAction = null)
        {
            services.AddCloudEventAggregatorCore();
            
            return services;
        }
    }
}
