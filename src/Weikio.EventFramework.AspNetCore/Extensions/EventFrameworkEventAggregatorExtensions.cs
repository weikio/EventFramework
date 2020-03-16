using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.AspNetCore.Extensions
{
    public static class EventFrameworkEventAggregatorExtensions
    {
        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, ICloudEventHandler handler)
        {
            return builder;
        }

        public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder) where THandlerType : class, ICloudEventHandler
        {
            builder.Services.AddTransient<ICloudEventHandler, THandlerType>();

            return builder;
        }
        
        public static IEventFrameworkBuilder AddHandler<TCloudEventDataType>(this IEventFrameworkBuilder builder, ICloudEventHandler<TCloudEventDataType> handler)
        {
            return builder;
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, string eventType, Func<ICloudEventContext, Task> handle)
        {
            return builder;
        }
        
        public static IEventFrameworkBuilder AddHandler<TCloudEventDataType>(this IEventFrameworkBuilder builder, string eventType, Func<ICloudEventContext<TCloudEventDataType>, Task> handle)
        {
            return builder;
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, string eventType, Func<IServiceProvider, ICloudEventContext, Task> handle)
        {
            return builder;
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Predicate<ICloudEventContext> canHandle, Func<ICloudEventContext, Task> handle)
        {
            return builder;
        }
        
        public static IEventFrameworkBuilder AddHandler<TCloudEventDataType>(this IEventFrameworkBuilder builder, Predicate<ICloudEventContext<TCloudEventDataType>> canHandle, Func<ICloudEventContext<TCloudEventDataType>, Task> handle)
        {
            return builder;
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Predicate<ICloudEventContext> canHandle, Func<IServiceProvider, ICloudEventContext, Task> handle)
        {
            return builder;
        }  
        
        public static IEventFrameworkBuilder AddHandler<TCloudEventDataType>(this IEventFrameworkBuilder builder, Predicate<CloudEvent> canHandle, Func<IServiceProvider, ICloudEventContext<TCloudEventDataType>, Task> handle)
        {
            return builder;
        }  

    }
}
