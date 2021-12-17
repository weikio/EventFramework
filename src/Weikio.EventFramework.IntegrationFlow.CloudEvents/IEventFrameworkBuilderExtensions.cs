using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
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

            var factory = new IntegrationFlowInstanceFactory(async serviceProvider =>
            {
                var flow = await flowBuilder.Build(serviceProvider);

                var options = new IntegrationFlowInstanceOptions()
                {
                    Id = "flowinstance_" + Guid.NewGuid()
                };
                
                var instanceFactory = serviceProvider.GetRequiredService<IIntegrationFlowInstanceFactory>();
                var result = await instanceFactory.Create(flow, options);

                return result;
            });

            services.AddIntegrationFlow(factory, new IntegrationFlowDefinition(flowBuilder.Name, flowBuilder.Description, flowBuilder.Version));

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

            var factory = new IntegrationFlowInstanceFactory(async serviceProvider =>
            {
                CloudEventsIntegrationFlowBase flowBase;

                if (configuration != null)
                {
                    flowBase = (CloudEventsIntegrationFlowBase)ActivatorUtilities.CreateInstance(serviceProvider, flowType, new object[] { configuration });
                }
                else
                {
                    flowBase = (CloudEventsIntegrationFlowBase)ActivatorUtilities.CreateInstance(serviceProvider, flowType);
                }

                configure?.DynamicInvoke(flowBase);

                var flow = await flowBase.Flow.Build(serviceProvider);
                var instanceFactory = serviceProvider.GetRequiredService<IIntegrationFlowInstanceFactory>();

                var options = new IntegrationFlowInstanceOptions()
                {
                    Id = "flowinstance_" + Guid.NewGuid(), Configuration = configuration, Configure = configure
                };
                
                var result = await instanceFactory.Create(flow, options);

                return result;
                // return instance.Flow.Build(serviceProvider);
            });

            services.AddIntegrationFlow(factory);

            return services;
        }

        public static IServiceCollection AddIntegrationFlow(this IServiceCollection services, IntegrationFlowInstanceFactory instanceFactory,
            IntegrationFlowDefinition definition = null)
        {
            services.AddCloudEventIntegrationFlows();

            services.AddSingleton<IntegrationFlowInstanceFactory>(instanceFactory);
            services.RegisterMyflow(instanceFactory, definition);

            return services;
        }

        public static IEventFrameworkBuilder AddIntegrationFlow(this IEventFrameworkBuilder builder, IntegrationFlowInstanceFactory instanceFactory)
        {
            var services = builder.Services;
            services.AddIntegrationFlow(instanceFactory);

            return builder;
        }

        public static IServiceCollection AddIntegrationFlow(this IServiceCollection services, IntegrationFlowInstance flowInstance)
        {
            services.AddCloudEventIntegrationFlows();

            var factory = new IntegrationFlowInstanceFactory(provider => Task.FromResult(flowInstance));
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

            // Task<IntegrationFlowInstance> CreateInstance(IServiceProvider serviceProvider)
            // {
            //     
            // }
            //
            // async Task<IntegrationFlowDefinition> CreateDefinition(IServiceProvider serviceProvider, CancellationToken cancellationToken)
            // {
            //     var catalog = new PluginFrameworkIntegrationFlowCatalog(typePluginCatalog);
            //     await catalog.Initialize(cancellationToken);
            //
            //     var plugin = catalog.Single();
            //
            //     var result = new IntegrationFlowDefinition(plugin.Name, plugin.Version);
            //
            //     var factory = new IntegrationFlowInstanceFactory(provider =>
            //     {
            //         CloudEventsIntegrationFlowBase instance;
            //
            //         if (configuration != null)
            //         {
            //             instance = (CloudEventsIntegrationFlowBase)ActivatorUtilities.CreateInstance(serviceProvider, flowType, new object[] { configuration });
            //         }
            //         else
            //         {
            //             instance = (CloudEventsIntegrationFlowBase)ActivatorUtilities.CreateInstance(serviceProvider, flowType);
            //         }
            //
            //         configure?.DynamicInvoke(instance);
            //
            //         return instance.Flow.Build(serviceProvider);
            //     });
            //     return result;
            // }

            services.AddSingleton<IIntegrationFlowCatalog>(provider =>
            {
                var catalog = new PluginFrameworkIntegrationFlowCatalog(typePluginCatalog);

                return catalog;
            });

            return services;
        }

        public static IServiceCollection RegisterMyflow(this IServiceCollection services, IntegrationFlowInstanceFactory instanceFactory)
        {
            // How can we generate the definition if no details is given? I suppose we could just use the guid for now
            var definition = new IntegrationFlowDefinition(Guid.NewGuid().ToString(), new Version(1, 0, 0));

            services.RegisterMyflow(instanceFactory, definition);

            return services;
        }

        public static IServiceCollection RegisterMyflow(this IServiceCollection services, IntegrationFlowInstanceFactory instanceFactory,
            IntegrationFlowDefinition definition)
        {
            var myFlow = new MyFlow() { Definition = definition, InstanceFactory = instanceFactory };

            services.AddSingleton(myFlow);

            if (definition != null)
            {
                services.AddSingleton(definition);
            }

            return services;
        }
    }

    public class MyFlow
    {
        public IntegrationFlowInstanceFactory InstanceFactory { get; set; }
        public IntegrationFlowDefinition Definition { get; set; }
    }
}
