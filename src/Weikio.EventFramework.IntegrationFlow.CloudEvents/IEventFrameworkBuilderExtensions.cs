using System;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public static class IEventFrameworkBuilderExtensions
    {
        public static IEventFrameworkBuilder AddIntegrationFlows(this IEventFrameworkBuilder builder)
        {
            var services = builder.Services;
            services.AddIntegrationFlows();

            return builder;
        }

        public static IServiceCollection AddIntegrationFlows(this IServiceCollection services)
        {
            services.AddHostedService<IntegrationFlowStartupService>();
            services.TryAddSingleton<ICloudEventsIntegrationFlowManager, DefaultCloudEventsIntegrationFlowManager>();

            return services;
        }

        public static IEventFrameworkBuilder AddIntegrationFlow(this IEventFrameworkBuilder builder, IntegrationFlowBuilder flowBuilder)
        {
            var services = builder.Services;
            services.AddIntegrationFlow(flowBuilder);

            return builder;
        }

        public static IEventFrameworkBuilder AddIntegrationFlow(this IEventFrameworkBuilder builder, IntegrationFlowInstance flowInstance)
        {
            var services = builder.Services;
            services.AddIntegrationFlow(flowInstance);

            return builder;
        }

        public static IServiceCollection AddIntegrationFlow(this IServiceCollection services, IntegrationFlowBuilder flowBuilder)
        {
            services.AddIntegrationFlows();
            services.AddSingleton(flowBuilder);

            return services;
        }

        public static IEventFrameworkBuilder AddIntegrationFlow<TFlowType>(this IEventFrameworkBuilder builder, MulticastDelegate configure = null,
            object configuration = null)
        {
            var services = builder.Services;

            services.AddIntegrationFlow(typeof(TFlowType), configure, configuration);

            return builder;
        }

        public static IEventFrameworkBuilder AddIntegrationFlow<TFlowType>(this IEventFrameworkBuilder builder, object configuration)
        {
            var services = builder.Services;

            services.AddIntegrationFlow(typeof(TFlowType), null, configuration);

            return builder;
        }

        public static IEventFrameworkBuilder AddIntegrationFlow(this IEventFrameworkBuilder builder, Type flowType, MulticastDelegate configure = null,
            object configuration = null)
        {
            var services = builder.Services;

            services.AddIntegrationFlow(flowType, configure, configuration);

            return builder;
        }

        public static IEventFrameworkBuilder AddIntegrationFlow(this IEventFrameworkBuilder builder, Type flowType, object configuration)
        {
            var services = builder.Services;

            services.AddIntegrationFlow(flowType, null, configuration);

            return builder;
        }

        public static IServiceCollection AddIntegrationFlow<TFlowType>(this IServiceCollection services, Action<TFlowType> configure = null,
            object configuration = null)
        {
            services.AddIntegrationFlow(typeof(TFlowType), configure, configuration);

            return services;
        }

        public static IServiceCollection AddIntegrationFlow<TFlowType>(this IServiceCollection services, object configuration)
        {
            services.AddIntegrationFlow(typeof(TFlowType), null, configuration);

            return services;
        }

        public static IServiceCollection AddIntegrationFlow(this IServiceCollection services, Type flowType, MulticastDelegate configure = null,
            object configuration = null)
        {
            services.AddIntegrationFlows();

            services.AddSingleton<CloudEventsIntegrationFlowFactory>(provider =>
            {
                var result = new CloudEventsIntegrationFlowFactory(serviceProvider =>
                {
                    CloudEventsIntegrationFlowBase instance;

                    if (configuration != null)
                    {
                        instance = (CloudEventsIntegrationFlowBase)ActivatorUtilities.CreateInstance(provider, flowType, new object[] { configuration });
                    }
                    else
                    {
                        instance = (CloudEventsIntegrationFlowBase)ActivatorUtilities.CreateInstance(provider, flowType);
                    }

                    configure?.DynamicInvoke(instance);

                    return instance.Flow.Build(serviceProvider);
                });

                return result;
            });

            return services;
        }

        public static IServiceCollection AddIntegrationFlow(this IServiceCollection services, IntegrationFlowInstance flowInstance)
        {
            services.AddIntegrationFlows();
            services.AddSingleton(flowInstance);

            return services;
        }

        public static IServiceCollection RegisterIntegrationFlow<TFlowType>(this IServiceCollection services)
        {
            var typeOfIntegrationFlow = typeof(TFlowType);
    
            var typePluginCatalog = new TypePluginCatalog(typeOfIntegrationFlow);

            services.AddSingleton<IIntegrationFlowCatalog>(provider =>
            {
                var catalog = new PluginFrameworkIntegrationFlowCatalog(typePluginCatalog);

                return catalog;
            });

            return services;
        }
    }
}
