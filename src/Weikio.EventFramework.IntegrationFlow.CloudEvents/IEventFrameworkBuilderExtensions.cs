using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public static class IEventFrameworkBuilderExtensions
    {
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
            services.AddCloudEventIntegrationFlows();
            
            var factory = new CloudEventsIntegrationFlowFactory(async serviceProvider =>
            {
                var flow = await flowBuilder.Build(serviceProvider);

                return flow;
            });

            services.AddIntegrationFlow(factory);
            
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
            services.AddCloudEventIntegrationFlows();

            var factory = new CloudEventsIntegrationFlowFactory(serviceProvider =>
            {
                CloudEventsIntegrationFlowBase instance;

                if (configuration != null)
                {
                    instance = (CloudEventsIntegrationFlowBase)ActivatorUtilities.CreateInstance(serviceProvider, flowType, new object[] { configuration });
                }
                else
                {
                    instance = (CloudEventsIntegrationFlowBase)ActivatorUtilities.CreateInstance(serviceProvider, flowType);
                }

                configure?.DynamicInvoke(instance);

                return instance.Flow.Build(serviceProvider);
            });

            services.AddIntegrationFlow(factory);
            
            return services;
        }
        
        public static IServiceCollection AddIntegrationFlow(this IServiceCollection services, CloudEventsIntegrationFlowFactory factory)
        {
            services.AddCloudEventIntegrationFlows();

            services.AddSingleton<CloudEventsIntegrationFlowFactory>(factory);
            services.RegisterMyflow(factory);
            
            return services;
        }
        
        public static IEventFrameworkBuilder AddIntegrationFlow(this IEventFrameworkBuilder builder, CloudEventsIntegrationFlowFactory factory)
        {
            var services = builder.Services;
            services.AddIntegrationFlow(factory);
            
            return builder;
        }

        public static IServiceCollection AddIntegrationFlow(this IServiceCollection services, IntegrationFlowInstance flowInstance)
        {
            services.AddCloudEventIntegrationFlows();

            var factory = new CloudEventsIntegrationFlowFactory(provider => Task.FromResult(flowInstance));
            services.AddIntegrationFlow(factory);
            
            return services;
        }

        public static IServiceCollection RegisterIntegrationFlow<TFlowType>(this IServiceCollection services)
        {
            services.RegisterIntegrationFlow(typeof(TFlowType));

            return services;
        }
        
        public static IServiceCollection RegisterIntegrationFlow(this IServiceCollection services, Type typeOfIntegrationFlow)
        {
            var typePluginCatalog = new TypePluginCatalog(typeOfIntegrationFlow);

            services.AddSingleton<IIntegrationFlowCatalog>(provider =>
            {
                var catalog = new PluginFrameworkIntegrationFlowCatalog(typePluginCatalog);

                return catalog;
            });

            return services;
        }
        
        public static IServiceCollection RegisterMyflow(this IServiceCollection services, CloudEventsIntegrationFlowFactory factory)
        {
            // How can we generate the definition if no details is given? I suppose we could just use the guid for now
            var definition = new IntegrationFlowDefinition(Guid.NewGuid().ToString(), new Version(1, 0, 0));

            services.RegisterMyflow(factory, definition);

            return services;
        }
        
        public static IServiceCollection RegisterMyflow(this IServiceCollection services, CloudEventsIntegrationFlowFactory factory, IntegrationFlowDefinition definition)
        {
            var myFlow = new MyFlow() { Definition = definition, Factory = factory };

            services.AddSingleton(myFlow);

            return services;
        }
    }

    public class MyFlow
    {
        public CloudEventsIntegrationFlowFactory Factory { get; set; }
        public IntegrationFlowDefinition Definition { get; set; }
    }
}
