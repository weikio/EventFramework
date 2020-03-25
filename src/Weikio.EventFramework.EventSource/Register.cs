using System;
using Microsoft.Extensions.DependencyInjection;
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
            
            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<QuartzJobRunner>();
            
            // Add our job
            services.AddSingleton<HelloWorldJob>();
            // services.AddSingleton(new JobSchedule(
            //     jobType: typeof(HelloWorldJob),
            //     cronExpression: "0/5 * * * * ?"));

            services.AddTransient<HelloWorld2>();
            
            // services.AddSingleton(new JobSchedule(
            //     jobType: typeof(HelloWorld2),
            //     cronExpression: "0/30 * * * * ?"));

            services.AddSingleton(new JobSchedule(
                jobType: typeof(HelloWorld2),
                cronExpression: "0/5 * * * * ?"));
            
            services.AddHostedService<QuartzHostedService>();
            return builder;
        }
    }
}
