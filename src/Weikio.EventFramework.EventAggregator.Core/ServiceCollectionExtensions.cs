using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventAggregator.Core.EventLinks;
using Weikio.EventFramework.EventAggregator.Core.EventLinks.EventLinkFactories;

namespace Weikio.EventFramework.EventAggregator.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IEventFrameworkBuilder AddCloudEventAggregatorCore(this IEventFrameworkBuilder builder, Action<CloudEventAggregatorOptions> setupAction = null)
        {
            AddCloudEventAggregatorCore(builder.Services, setupAction);

            return builder;
        }

        public static IServiceCollection AddCloudEventAggregatorCore(this IServiceCollection services, Action<CloudEventAggregatorOptions> setupAction = null)
        {
            services.TryAddSingleton<ICloudEventAggregator, CloudEventAggregator>();
            services.TryAddSingleton<EventLinkInitializer>();
            services.TryAddTransient<IEventLinkRunner, DefaultEventLinkRunner>();

            services.TryAddSingleton<ITypeToEventLinksConverter, DefaultTypeToEventLinksConverter>();
            services.AddHostedService<EventLinkStartupTask>();

            var options = new CloudEventAggregatorOptions();
            setupAction?.Invoke(options);

            var conf = Options.Create(options);
            services.AddSingleton<IOptions<CloudEventAggregatorOptions>>(conf);
            
            foreach (var typeToEventLinksFactoryType in options.TypeToEventLinksHandlerTypes)
            {
                services.TryAddTransient(typeof(ITypeToHandlers), typeToEventLinksFactoryType);
                services.TryAddTransient(typeToEventLinksFactoryType);
            }

            return services;
        }
    }
}
