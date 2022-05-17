using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventFlow.Abstractions;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public static class IEventFrameworkBuilderExtensions
    {
        public static IEventFrameworkBuilder AddEventFlow(this IEventFrameworkBuilder builder, IEventFlowBuilder flowBuilder)
        {
            var services = builder.Services;
            services.AddEventFlow(flowBuilder);

            return builder;
        }

        public static IEventFrameworkBuilder AddEventFlow(this IEventFrameworkBuilder builder, EventFlowInstance flowInstance)
        {
            var services = builder.Services;
            services.AddEventFlow(flowInstance);

            return builder;
        }

        public static IServiceCollection AddEventFlow(this IServiceCollection services, IEventFlowBuilder flowBuilder)
        {
            services.AddCloudEventIntegrationFlows();

            var factory = new EventFlowInstanceFactory(async serviceProvider =>
            {
                var flow = await flowBuilder.Build(serviceProvider);

                var options = new EventFlowInstanceOptions()
                {
                    Id = Guid.NewGuid().ToString()
                };
                
                var instanceFactory = serviceProvider.GetRequiredService<IEventFlowInstanceFactory>();
                var result = await instanceFactory.Create(flow, options);

                return result;
            });

            services.AddEventFlow(factory, new EventFlowDefinition(flowBuilder.Name, flowBuilder.Description, flowBuilder.Version));

            return services;
        }

        public static IEventFrameworkBuilder AddEventFlow<TFlowType>(this IEventFrameworkBuilder builder, MulticastDelegate configure = null,
            object configuration = null)
        {
            var services = builder.Services;

            services.AddEventFlow(typeof(TFlowType), configure, configuration);

            return builder;
        }

        public static IEventFrameworkBuilder AddEventFlow<TFlowType>(this IEventFrameworkBuilder builder, object configuration)
        {
            var services = builder.Services;

            services.AddEventFlow(typeof(TFlowType), null, configuration);

            return builder;
        }

        public static IEventFrameworkBuilder AddEventFlow(this IEventFrameworkBuilder builder, Type flowType, MulticastDelegate configure = null,
            object configuration = null)
        {
            var services = builder.Services;

            services.AddEventFlow(flowType, configure, configuration);

            return builder;
        }

        public static IEventFrameworkBuilder AddEventFlow(this IEventFrameworkBuilder builder, Type flowType, object configuration)
        {
            var services = builder.Services;

            services.AddEventFlow(flowType, null, configuration);

            return builder;
        }

        public static IServiceCollection AddEventFlow<TFlowType>(this IServiceCollection services, Action<TFlowType> configure = null,
            object configuration = null)
        {
            services.AddEventFlow(typeof(TFlowType), configure, configuration);

            return services;
        }

        public static IServiceCollection AddEventFlow<TFlowType>(this IServiceCollection services, object configuration)
        {
            services.AddEventFlow(typeof(TFlowType), null, configuration);

            return services;
        }

        public static IServiceCollection AddEventFlow(this IServiceCollection services, Type flowType, MulticastDelegate configure = null,
            object configuration = null)
        {
            services.AddCloudEventIntegrationFlows();

            var factory = new EventFlowInstanceFactory(async serviceProvider =>
            {
                CloudEventFlowBase flowBase;

                if (configuration != null)
                {
                    flowBase = (CloudEventFlowBase)ActivatorUtilities.CreateInstance(serviceProvider, flowType, new object[] { configuration });
                }
                else
                {
                    flowBase = (CloudEventFlowBase)ActivatorUtilities.CreateInstance(serviceProvider, flowType);
                }

                configure?.DynamicInvoke(flowBase);

                var flow = await flowBase.Flow.Build(serviceProvider);
                var instanceFactory = serviceProvider.GetRequiredService<IEventFlowInstanceFactory>();

                var options = new EventFlowInstanceOptions()
                {
                    Id = Guid.NewGuid().ToString(), Configuration = configuration, Configure = configure
                };
                
                var result = await instanceFactory.Create(flow, options);

                return result;
                // return instance.Flow.Build(serviceProvider);
            });

            services.AddEventFlow(factory);

            return services;
        }

        public static IServiceCollection AddEventFlow(this IServiceCollection services, EventFlowInstanceFactory instanceFactory,
            EventFlowDefinition definition = null)
        {
            services.AddCloudEventIntegrationFlows();

            services.AddSingleton<EventFlowInstanceFactory>(instanceFactory);
            services.RegisterMyflow(instanceFactory, definition);

            return services;
        }

        public static IEventFrameworkBuilder AddEventFlow(this IEventFrameworkBuilder builder, EventFlowInstanceFactory instanceFactory)
        {
            var services = builder.Services;
            services.AddEventFlow(instanceFactory);

            return builder;
        }

        public static IServiceCollection AddEventFlow(this IServiceCollection services, EventFlowInstance flowInstance)
        {
            services.AddCloudEventIntegrationFlows();

            var factory = new EventFlowInstanceFactory(provider => Task.FromResult(flowInstance));
            services.AddEventFlow(factory);

            return services;
        }

        public static IServiceCollection RegisterEventFlow<TFlowType>(this IServiceCollection services)
        {
            services.RegisterEventFlow(typeof(TFlowType));

            return services;
        }

        public static IServiceCollection RegisterEventFlow(this IServiceCollection services, Type typeOfIntegrationFlow)
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

            services.AddSingleton<IEventFlowCatalog>(provider =>
            {
                var catalog = new PluginFrameworkEventFlowCatalog(typePluginCatalog);

                return catalog;
            });

            return services;
        }

        public static IServiceCollection RegisterMyflow(this IServiceCollection services, EventFlowInstanceFactory instanceFactory)
        {
            // How can we generate the definition if no details is given? I suppose we could just use the guid for now
            var definition = new EventFlowDefinition(Guid.NewGuid().ToString(), new Version(1, 0, 0));

            services.RegisterMyflow(instanceFactory, definition);

            return services;
        }

        public static IServiceCollection RegisterMyflow(this IServiceCollection services, EventFlowInstanceFactory instanceFactory,
            EventFlowDefinition definition)
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
        public EventFlowInstanceFactory InstanceFactory { get; set; }
        public EventFlowDefinition Definition { get; set; }
    }
}
