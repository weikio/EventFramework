using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Configuration;

namespace Weikio.EventFramework.EventSource
{
    public static class Register
    {
        public static IEventFrameworkBuilder AddEventSources(this IEventFrameworkBuilder builder, Action<EventFrameworkOptions> setupAction = null)
        {
            var services = builder.Services;

            services.TryAddSingleton<IJobFactory, JobFactory>();
            services.TryAddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.TryAddSingleton<QuartzJobRunner>();

            services.TryAddSingleton<HelloWorldJob>();
            services.TryAddTransient<Wrapper>();

            if (services.All(x => x.ImplementationType != typeof(QuartzHostedService)))
            {
                services.AddHostedService<QuartzHostedService>();
            }

            return builder;
        }

        public static IEventFrameworkBuilder AddSource(this IEventFrameworkBuilder builder, Func<object, Task<(object, object)>> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            builder.AddEventSources();

            if (pollingFrequency == null && string.IsNullOrWhiteSpace(cronExpression))
            {
                // Todo: Default form options
                pollingFrequency = TimeSpan.FromSeconds(30);
            }

            builder.Services.AddSingleton(provider =>
            {
                var wrapper = provider.GetRequiredService<Wrapper>();
                var result = wrapper.Create(action);
                
                var schedule = new JobSchedule(result, pollingFrequency, cronExpression);

                return schedule;
            });

            return builder;
        }
        
        public static IEventFrameworkBuilder AddSource<TStateType>(this IEventFrameworkBuilder builder, Func<TStateType, (object, TStateType)> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            Func<TStateType, Task<(object, TStateType)>> taskAction = state => Task.FromResult(action(state));

            return builder.AddSource(taskAction, pollingFrequency, cronExpression, configure);
        }
        
        public static IEventFrameworkBuilder AddSource<TStateType>(this IEventFrameworkBuilder builder, Func<TStateType, Task<(object, TStateType)>> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            builder.AddEventSources();

            if (pollingFrequency == null && string.IsNullOrWhiteSpace(cronExpression))
            {
                // Todo: Default form options
                pollingFrequency = TimeSpan.FromSeconds(30);
            }

            builder.Services.AddSingleton(provider =>
            {
                var wrapper = provider.GetRequiredService<Wrapper>();
                var result = wrapper.Create(action);
                
                var schedule = new JobSchedule(result, pollingFrequency, cronExpression);

                return schedule;
            });

            return builder;
        }
        
        public static IEventFrameworkBuilder AddSource(this IEventFrameworkBuilder builder, Func<Task<List<object>>> action, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            builder.AddEventSources();

            if (pollingFrequency == null && string.IsNullOrWhiteSpace(cronExpression))
            {
                // Todo: Default form options
                pollingFrequency = TimeSpan.FromSeconds(30);
            }

            builder.Services.AddSingleton(provider =>
            {
                var wrapper = provider.GetRequiredService<Wrapper>();
                var result = wrapper.Create(action);
                
                var schedule = new JobSchedule(result, pollingFrequency, cronExpression);

                return schedule;
            });

            return builder;
        }

        public class Wrapper
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly ICloudEventPublisher _publisher;

            public Wrapper(IServiceProvider serviceProvider, ICloudEventPublisher publisher)
            {
                _serviceProvider = serviceProvider;
                _publisher = publisher;
            }

            public Func<object, Task<object>> Create(Func<object, Task<(object CloudEvent, object UpdatedState)>> action)
            {
                var result = new Func<object, Task<object>>(async state =>
                {
                    try
                    {
                        var cloudEvent = action(state);
                        var res = await cloudEvent;

                        if (res.CloudEvent != null)
                        {
                            await _publisher.Publish(res.CloudEvent);
                        }

                        return res.UpdatedState;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);

                        throw;
                    }
                });

                return result;
            }
            
            public Func<TStateType, Task<TStateType>> Create<TStateType>(Func<TStateType, Task<(object CloudEvent, TStateType UpdatedState)>> action)
            {
                var result = new Func<TStateType, Task<TStateType>>(async state =>
                {
                    try
                    {
                        var cloudEvent = action(state);
                        var res = await cloudEvent;

                        if (res.CloudEvent != null)
                        {
                            await _publisher.Publish(res.CloudEvent);
                        }

                        return res.UpdatedState;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);

                        throw;
                    }
                });

                return result;
            }

            
            public Func<object, Task> Create(Func<Task<List<object>>> action)
            {
                var result = new Func<object, Task>(async state =>
                {
                    var cloudEvent = action();
                    var res = await cloudEvent;

                    if (res == null)
                    {
                        return;
                    }

                    await _publisher.Publish(res);
                });

                return result;
            }
        }

        public static IEventFrameworkBuilder AddSource(this IEventFrameworkBuilder builder, Type sourceType, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null)
        {
            builder.AddEventSources();

            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            if (pollingFrequency == null && string.IsNullOrWhiteSpace(cronExpression))
            {
                // Todo: Default form options
                pollingFrequency = TimeSpan.FromSeconds(30);
            }

            if (builder.Services.All(x => x.ServiceType != sourceType))
            {
                builder.Services.AddTransient(sourceType);
            }

            builder.Services.AddSingleton(provider =>
            {
                var action = new Func<IServiceProvider, IJobExecutionContext, Task>((serviceProvider, context) =>
                {
                    dynamic job = serviceProvider.GetRequiredService(sourceType);

                    if (configure != null)
                    {
                        configure.DynamicInvoke(new[] { job });
                    }

                    var stateProperty = sourceType.GetProperties().FirstOrDefault(x =>
                        x.CanRead && x.CanWrite && string.Equals(x.Name, "State", StringComparison.InvariantCultureIgnoreCase));

                    if (stateProperty != null)
                    {
                        var statePropertyType = stateProperty.PropertyType;

                        if (!context.JobDetail.JobDataMap.ContainsKey("state"))
                        {
                            context.JobDetail.JobDataMap["state"] = GetDefaultValue(statePropertyType);
                        }

                        var currentState = context.JobDetail.JobDataMap["state"];

                        stateProperty.SetValue(job, currentState);
                    }

                    // var r = new Func<Task>(async () =>
                    // {
                    //     var result = await job.Execute();
                    //
                    //     if (stateProperty != null)
                    //     {
                    //         var updatedState = stateProperty.GetValue(job);
                    //         context.JobDetail.JobDataMap["state"] = updatedState;
                    //     }
                    //
                    //     return result;
                    // });
                    // var result =  await job.Execute();
                    //
                    // if (stateProperty != null)
                    // {
                    //     var updatedState = stateProperty.GetValue(job);
                    //     context.JobDetail.JobDataMap["state"] = updatedState;
                    // }
                    //
                    // return result;

                    return job.Execute();
                });

                var schedule = new JobSchedule(action, pollingFrequency, cronExpression);

                return schedule;
            });

            return builder;
        }

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
