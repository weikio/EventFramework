using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

            services.TryAddSingleton<HelloWorldJob>();
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

        public static IEventFrameworkBuilder AddSource(this IEventFrameworkBuilder builder, Func<object, Task<(object, object)>> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            builder.Services.AddSource(action, pollingFrequency, cronExpression, configure);

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
            var taskAction = Task.FromResult(new List<object>(){action});

            Task<List<object>> FuncTaskAction() => taskAction;

            services.AddSource(FuncTaskAction, pollingFrequency, cronExpression, configure);

            return services;
        }

        public static IServiceCollection AddSource(this IServiceCollection services, Func<object, Task<(object, object)>> action,
            TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
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

            services.AddOptions<JobOptions>(id.ToString())
                .Configure<EventSourceActionWrapper>((jobOptions, wrapper) =>
                {
                    var act = wrapper.Create(action);
                    jobOptions.Action = act;
                });

            return services;
        }
        
        public static IEventFrameworkBuilder AddSource<TStateType>(this IEventFrameworkBuilder builder, Func<TStateType, (object, TStateType)> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            builder.Services.AddSource<TStateType>(action, pollingFrequency, cronExpression, configure);

            return builder;
        }
        
        public static IServiceCollection AddSource<TStateType>(this IServiceCollection services, Func<TStateType, (object, TStateType)> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            Func<TStateType, Task<(object, TStateType)>> taskAction = state => Task.FromResult(action(state));

            return services.AddSource<TStateType>(taskAction, pollingFrequency, cronExpression, configure);
        }
        
        public static IEventFrameworkBuilder AddSource<TStateType>(this IEventFrameworkBuilder builder, Func<TStateType, Task<(object, TStateType)>> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            builder.Services.AddSource<TStateType>(action, pollingFrequency, cronExpression, configure);
            
            return builder;
        }
        
        public static IServiceCollection AddSource<TStateType>(this IServiceCollection services, Func<TStateType, Task<(object, TStateType)>> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
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

            services.AddOptions<JobOptions>(id.ToString())
                .Configure<EventSourceActionWrapper>((jobOptions, wrapper) =>
                {
                    var act = wrapper.Create(action);
                    jobOptions.Action = act;
                });

            return services;
        }
        
        public static IServiceCollection AddSource<TEventSource>(this IServiceCollection services, TimeSpan? pollingFrequency = null, string cronExpression = null, Action<TEventSource> configure = null)
        {
            services.AddCloudEventSources();

            if (pollingFrequency == null && string.IsNullOrWhiteSpace(cronExpression))
            {
                // Todo: Default from options
                pollingFrequency = TimeSpan.FromSeconds(30);
            }

            var id = Guid.Parse("652d9b12-0780-42a1-b3c2-2643bb4f52a8");

            services.AddTransient(typeof(TEventSource));
            
            services.AddTransient(provider =>
            {
                var schedule = new JobSchedule(id, pollingFrequency, cronExpression);

                return schedule;
            });

            services.AddTransient(provider =>
            {
                var eventSourceActionWrapperFactory = new EventSourceActionWrapperFactory(typeof(TEventSource), id);

                return eventSourceActionWrapperFactory;
            });

            // services.AddOptions<JobOptions>(id.ToString())
            //     .Configure<EventSourceActionWrapper>((jobOptions, wrapper) =>
            //     {
            //         var act = wrapper.Create(typeof(TEventSource));
            //         jobOptions.Action = act;
            //     });

            return services;
        }

        
        public static IEventFrameworkBuilder AddSource(this IEventFrameworkBuilder builder, Func<Task<List<object>>> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            builder.Services.AddSource(action, pollingFrequency, cronExpression, configure);

            return builder;
        }
        
        public static IServiceCollection AddSource(this IServiceCollection services, Func<Task<List<object>>> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
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

            services.AddOptions<JobOptions>(id.ToString())
                .Configure<EventSourceActionWrapper>((jobOptions, wrapper) =>
                {
                    var act = wrapper.Create(action);
                    jobOptions.Action = act;
                });
            
            return services;
        }

        // public static IEventFrameworkBuilder AddSource(this IEventFrameworkBuilder builder, Type sourceType, TimeSpan? pollingFrequency = null,
        //     string cronExpression = null, MulticastDelegate configure = null)
        // {
        //     builder.AddEventSources();
        //
        //     if (sourceType == null)
        //     {
        //         throw new ArgumentNullException(nameof(sourceType));
        //     }
        //
        //     if (pollingFrequency == null && string.IsNullOrWhiteSpace(cronExpression))
        //     {
        //         // Todo: Default from options
        //         pollingFrequency = TimeSpan.FromSeconds(30);
        //     }
        //
        //     if (builder.Services.All(x => x.ServiceType != sourceType))
        //     {
        //         builder.Services.AddTransient(sourceType);
        //     }
        //
        //     builder.Services.AddSingleton(provider =>
        //     {
        //         var action = new Func<IServiceProvider, IJobExecutionContext, Task>((serviceProvider, context) =>
        //         {
        //             dynamic job = serviceProvider.GetRequiredService(sourceType);
        //
        //             if (configure != null)
        //             {
        //                 configure.DynamicInvoke(new[] { job });
        //             }
        //
        //             var stateProperty = sourceType.GetProperties().FirstOrDefault(x =>
        //                 x.CanRead && x.CanWrite && string.Equals(x.Name, "State", StringComparison.InvariantCultureIgnoreCase));
        //
        //             if (stateProperty != null)
        //             {
        //                 var statePropertyType = stateProperty.PropertyType;
        //
        //                 if (!context.JobDetail.JobDataMap.ContainsKey("state"))
        //                 {
        //                     context.JobDetail.JobDataMap["state"] = GetDefaultValue(statePropertyType);
        //                 }
        //
        //                 var currentState = context.JobDetail.JobDataMap["state"];
        //
        //                 stateProperty.SetValue(job, currentState);
        //             }
        //
        //             // var r = new Func<Task>(async () =>
        //             // {
        //             //     var result = await job.Execute();
        //             //
        //             //     if (stateProperty != null)
        //             //     {
        //             //         var updatedState = stateProperty.GetValue(job);
        //             //         context.JobDetail.JobDataMap["state"] = updatedState;
        //             //     }
        //             //
        //             //     return result;
        //             // });
        //             // var result =  await job.Execute();
        //             //
        //             // if (stateProperty != null)
        //             // {
        //             //     var updatedState = stateProperty.GetValue(job);
        //             //     context.JobDetail.JobDataMap["state"] = updatedState;
        //             // }
        //             //
        //             // return result;
        //
        //             return job.Execute();
        //         });
        //
        //         var schedule = new JobSchedule(action, pollingFrequency, cronExpression);
        //
        //         return schedule;
        //     });
        //
        //     return builder;
        // }

        static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t);
            }

            if (t.GetConstructor(Type.EmptyTypes) != null)
            {
                return Activator.CreateInstance(t);
            }

            return null;
        }
    }
}
