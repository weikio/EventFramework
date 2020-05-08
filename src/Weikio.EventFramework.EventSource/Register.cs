using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Weikio.EventFramework.Abstractions.DependencyInjection;

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

            if (services.All(x => x.ImplementationType != typeof(QuartzHostedService)))
            {
                services.AddHostedService<EventSourceActionWrapperUnwrapperHost>();
            }

            services.TryAddSingleton<IJobFactory, JobFactory>();
            services.TryAddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.TryAddSingleton<QuartzJobRunner>();
            services.TryAddSingleton<JobScheduleService>();

            services.TryAddTransient<EventSourceActionWrapper>();

            if (services.All(x => x.ImplementationType != typeof(QuartzHostedService)))
            {
                services.AddHostedService<QuartzHostedService>();
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
            string cronExpression = null, MulticastDelegate configure = null, Type eventSourceType = null)
        {
            services.AddCloudEventSources();

            if (pollingFrequency == null && string.IsNullOrWhiteSpace(cronExpression))
            {
                // Todo: Default from options
                pollingFrequency = TimeSpan.FromSeconds(30);
            }

            var id = Guid.NewGuid();

            services.AddSingleton(provider =>
            {
                var schedule = new JobSchedule(id, pollingFrequency, cronExpression);

                return schedule;
            });

            if (eventSourceType != null)
            {
                services.TryAddTransient(eventSourceType);
                services.AddTransient(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<EventSourceActionWrapperFactory>>();
                    var eventSourceActionWrapperFactory = new EventSourceActionWrapperFactory(eventSourceType, id, logger);
                
                    return eventSourceActionWrapperFactory;
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
