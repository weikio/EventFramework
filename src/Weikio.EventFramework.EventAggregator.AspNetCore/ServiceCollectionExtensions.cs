using System;
using Microsoft.Extensions.DependencyInjection;
using Weikio.AspNetCore.StartupTasks;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventAggregator.Core;

namespace Weikio.EventFramework.EventAggregator.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IEventFrameworkBuilder AddEventAggregator(this IEventFrameworkBuilder builder, Action<CloudEventAggregatorOptions> setupAction = null)
        {
            AddEventAggregator(builder.Services, setupAction);

            return builder;
        }

        public static IServiceCollection AddEventAggregator(this IServiceCollection services, Action<CloudEventAggregatorOptions> setupAction = null)
        {
            services.AddEventAggregatorCore();
            services.AddStartupTasks();

            return services;
        }
    }
}
