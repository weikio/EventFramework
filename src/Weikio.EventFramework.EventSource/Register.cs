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
using Weikio.EventFramework.EventSource.EventSourceWrapping;
using Weikio.EventFramework.EventSource.LongPolling;
using Weikio.EventFramework.EventSource.Polling;

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

            if (services.All(x => x.ImplementationType != typeof(DefaultPollingEventSourceHostedService)))
            {
                services.AddHostedService<EventSourceActionWrapperUnwrapperHost>();
            }

            if (services.All(x => x.ImplementationType != typeof(LongPollingHostedServiceCreator)))
            {
                services.AddHostedService<LongPollingHostedServiceCreator>();
            }

            services.TryAddSingleton<IJobFactory, DefaultJobFactory>();
            services.TryAddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.TryAddSingleton<PollingJobRunner>();
            services.TryAddSingleton<PollingScheduleService>();
            services.TryAddTransient<IActionWrapper, DefaultActionWrapper>();
            services.TryAddSingleton<LongPollingService>();

            services.TryAddTransient<EventSourceActionWrapper>();
            services.TryAddTransient<ILongPollingEventSourceHost, DefaultLongPollingEventSourceHost>();

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

        public static IServiceCollection AddSourceInner(this IServiceCollection services, MulticastDelegate action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Type eventSourceType = null, object eventSourceInstance = null)
        {
            services.AddCloudEventSources();

            var id = Guid.NewGuid();

            if (eventSourceType == null && eventSourceInstance != null)
            {
                eventSourceType = eventSourceInstance.GetType();
            }

            var isHostedService = eventSourceType != null && typeof(IHostedService).IsAssignableFrom(eventSourceType);

            var requiredPollingJob = isHostedService == false;

            if (requiredPollingJob)
            {
                services.AddSingleton(provider =>
                {
                    if (pollingFrequency == null)
                    {
                        var optionsManager = provider.GetService<IOptionsMonitor<PollingOptions>>();
                        var options = optionsManager.CurrentValue;

                        pollingFrequency = options.PollingFrequency;
                    }
                
                    var schedule = new PollingSchedule(id, pollingFrequency, cronExpression);

                    return schedule;
                });
            }
            
            if (isHostedService)
            {
                services.AddTransient(typeof(IHostedService), provider =>
                {
                    var inst = ActivatorUtilities.CreateInstance(provider, eventSourceType);

                    if (configure != null)
                    {
                        configure.DynamicInvoke(inst);
                    }
                    
                    return inst;
                });
            }
            else if (eventSourceType != null)
            {
                services.TryAddTransient(eventSourceType);

                services.AddTransient(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<TypeToEventSourceFactory>>();
                    var factory = new TypeToEventSourceFactory(eventSourceType, id, logger, eventSourceInstance);

                    return factory;
                });
            }
            else
            {
                services.AddOptions<JobOptions>(id.ToString())
                    .Configure<EventSourceActionWrapper>((jobOptions, wrapper) =>
                    {
                        var wrapped = wrapper.Wrap(action);
                        jobOptions.Action = wrapped.Action;
                        jobOptions.ContainsState = wrapped.ContainsState;
                    });
            }

            return services;
        }
    }
}

