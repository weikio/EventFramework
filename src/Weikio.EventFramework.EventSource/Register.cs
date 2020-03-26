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
            // services.TryAddTransient<HelloWorld2>();

            // var jobSchedule = new JobSchedule(typeof(HelloWorld2), "0/30 * * * * ?") { Configure = new Action<HelloWorld2>(x => x.Folder = @"c:\temp\long") };
            // services.AddSingleton(jobSchedule);
            //
            // var implementationInstance =
            //     new JobSchedule(typeof(HelloWorld2), TimeSpan.FromSeconds(5)) { Configure = new Action<HelloWorld2>(x => x.Folder = @"c:\short") };
            //
            // services.AddSingleton(implementationInstance);

            if (services.All(x => x.ImplementationType != typeof(QuartzHostedService)))
            {
                services.AddHostedService<QuartzHostedService>();
            }

            return builder;
        }
        
        public static IEventFrameworkBuilder AddSource(this IEventFrameworkBuilder builder, Type sourceType, TimeSpan? pollingFrequency = null, string cronExpression = null, MulticastDelegate configure = null)
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
