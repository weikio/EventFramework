using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.EventSource.EventSourceWrapping;
using Weikio.EventFramework.EventSource.LongPolling;
using Weikio.EventFramework.EventSource.Polling;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.EventFramework.EventSource
{
    public static class Register
    {
        public static IEventFrameworkBuilder AddCloudEventSources(this IEventFrameworkBuilder builder, Action<EventSourceOptions> setupAction = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (builder.Services == null)
            {
                throw new ArgumentNullException(nameof(builder.Services));
            }

            builder.Services.AddCloudEventSources(setupAction);

            return builder;
        }

        public static IServiceCollection AddCloudEventSources(this IServiceCollection services, Action<EventSourceOptions> setupAction = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (services.All(x => x.ImplementationType != typeof(EventSourceInstanceStartupHandler)))
            {
                services.AddHostedService<EventSourceInstanceStartupHandler>();
            }

            services.TryAddSingleton<IJobFactory, DefaultJobFactory>();

            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.UseInMemoryStore();
            });

            services.TryAddSingleton<PollingJobRunner>();
            services.TryAddSingleton<PollingScheduleService>();
            services.TryAddTransient<IActionWrapper, DefaultActionWrapper>();
            services.TryAddSingleton<IEventSourceFactory, DefaultEventSourceFactory>();

            services.TryAddTransient<EventSourceActionWrapper>();
            services.TryAddTransient<ILongPollingEventSourceHost, DefaultLongPollingEventSourceHost>();

            services.TryAddSingleton<EventSourceChangeToken>();
            services.TryAddSingleton<EventSourceChangeNotifier>();
            services.TryAddSingleton<EventSourceChangeProvider>();
            services.TryAddSingleton<IEventSourceProvider, DefaultEventSourceProvider>();
            services.TryAddSingleton<IEventSourceInstanceManager, DefaultEventSourceInstanceManager>();
            services.TryAddSingleton<IEventSourceInstanceFactory, DefaultEventSourceInstanceFactory>();
            services.TryAddSingleton<ICloudEventPublisherFactory, DefaultCloudEventPublisherFactory>();
            services.TryAddTransient<ICloudEventPublisherBuilder, DefaultCloudEventPublisherBuilder>();
            services.TryAddSingleton<IEventSourceCatalog, PluginEventSourceCatalog>();
            services.TryAddSingleton<IEventSourceDefinitionProvider, DefaultEventSourceDefinitionProvider>();
            services.TryAddSingleton<IEventSourceDefinitionConfigurationTypeProvider, DefaultEventSourceDefinitionConfigurationTypeProvider>();
            services.TryAddSingleton<ITypeToEventSourceTypeProvider, DefaultTypeToEventSourceTypeProvider>();
            services.TryAddSingleton<IEventSourceInstanceStorageFactory, DefaultEventSourceInstanceStorageFactory>();
            services.TryAddTransient<IPersistableEventSourceInstanceDataStore, FileEventSourceInstanceDataStore>();

            if (services.All(x => x.ImplementationType != typeof(DefaultPollingEventSourceHostedService)))
            {
                services.AddHostedService<DefaultPollingEventSourceHostedService>();
            }

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return services;
        }

        public static IEventFrameworkBuilder AddSource(this IEventFrameworkBuilder builder, Func<object, Task<(object, object)>> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            builder.Services.AddSourceInner(action, pollingFrequency, cronExpression, configure);

            return builder;
        }

        public static IEventFrameworkBuilder AddSource(this IEventFrameworkBuilder builder, Func<object> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            builder.Services.AddSource(action, pollingFrequency, cronExpression, configure);

            return builder;
        }

        public static IServiceCollection AddSource(this IServiceCollection services, Func<object> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            services.AddSourceInner(action, pollingFrequency, cronExpression, configure);

            return services;
        }

        public static IServiceCollection AddSource(this IServiceCollection services, object instance, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            services.AddSourceInner(null, pollingFrequency, cronExpression, configure, null, instance);

            return services;
        }

        public static IEventFrameworkBuilder AddSource<TStateType>(this IEventFrameworkBuilder builder, Func<TStateType, (object, TStateType)> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            builder.Services.AddSource<TStateType>(action, pollingFrequency, cronExpression, configure);

            return builder;
        }

        public static IServiceCollection AddSource<TStateType>(this IServiceCollection services, Func<TStateType, (object, TStateType)> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            Func<TStateType, Task<(object, TStateType)>> taskAction = state => Task.FromResult(action(state));

            return services.AddSourceInner(taskAction, pollingFrequency, cronExpression, configure);
        }

        public static IEventFrameworkBuilder AddSource<TStateType>(this IEventFrameworkBuilder builder, Func<TStateType, Task<(object, TStateType)>> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            builder.Services.AddSourceInner(action, pollingFrequency, cronExpression, configure);

            return builder;
        }

        public static IServiceCollection AddSource<TEventSource>(this IServiceCollection services, TimeSpan? pollingFrequency = null,
            string cronExpression = null, Action<TEventSource> configure = null)
        {
            services.AddSourceInner(null, pollingFrequency, cronExpression, configure, typeof(TEventSource));

            return services;
        }

        public static IEventFrameworkBuilder AddSource(this IEventFrameworkBuilder builder, Func<Task<List<object>>> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            builder.Services.AddSource(action, pollingFrequency, cronExpression, configure);

            return builder;
        }

        public static IServiceCollection AddSourceInner(this IServiceCollection services, MulticastDelegate action = null, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Type eventSourceType = null, object eventSourceInstance = null)
        {
            services.AddCloudEventSources();

            return services;
        }

        public static IEventFrameworkBuilder AddEventSource<TEventSourceType>(this IEventFrameworkBuilder builder,
            Action<EventSourceInstanceOptions> configureInstance = null)
        {
            var services = builder.Services;

            services.AddEventSource<TEventSourceType>(configureInstance);

            return builder;
        }

        public static IServiceCollection AddEventSource<TEventSourceType>(this IServiceCollection services,
            Action<EventSourceInstanceOptions> configureInstance = null)
        {
            services.AddCloudEventSources();

            var typePluginCatalog = new TypePluginCatalog(typeof(TEventSourceType));

            services.AddSingleton<IEventSourceCatalog>(provider =>
            {
                var catalog = new PluginFrameworkEventSourceCatalog(typePluginCatalog);

                return catalog;
            });

            if (configureInstance != null)
            {
                services.AddSingleton(provider =>
                {
                    var options = new EventSourceInstanceOptions();
                    configureInstance(options);

                    if (options.EventSourceDefinition == null)
                    {
                        var definition = typePluginCatalog.Single();
                        options.EventSourceDefinition = definition.Name;
                    }

                    return options;
                });
            }

            return services;
        }

        public static IEventFrameworkBuilder AddEventSource(this IEventFrameworkBuilder builder, EventSourceDefinition eventSourceDefinition,
            MulticastDelegate action = null,
            Type eventSourceType = null, object eventSourceInstance = null)
        {
            builder.Services.AddEventSource(eventSourceDefinition, action, eventSourceType, eventSourceInstance);

            return builder;
        }

        public static IServiceCollection AddEventSource(this IServiceCollection services, EventSourceDefinition eventSourceDefinition,
            MulticastDelegate action = null,
            Type eventSourceType = null, object eventSourceInstance = null)
        {
            services.AddCloudEventSources();

            services.AddSingleton<IEventSourceCatalog>(provider =>
            {
                var factory = provider.GetRequiredService<IEventSourceFactory>();
                var eventSourceDefinitionProvider = provider.GetRequiredService<IEventSourceDefinitionProvider>();
                var definition = eventSourceDefinitionProvider.Get(eventSourceDefinition.Name, eventSourceDefinition.Version);

                var eventSource = factory.Create(definition, action, eventSourceType, eventSourceInstance);

                var catalog = new EventSourceCatalog { eventSource };

                return catalog;
            });

            return services;
        }
    }
}
