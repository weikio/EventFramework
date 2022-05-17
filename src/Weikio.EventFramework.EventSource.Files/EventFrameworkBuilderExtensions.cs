using System;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.EventSource.SDK;

namespace Weikio.EventFramework.EventSource.Files
{
    public static class EventFrameworkBuilderExtensions
    {
        public static IEventFrameworkBuilder AddFileEventSource(this IEventFrameworkBuilder builder,
            Action<EventSourceInstanceOptions> configureInstance = null)
        {
            var services = builder.Services;

            services.AddFileEventSource(configureInstance);

            return builder;
        } 
        
        public static IServiceCollection AddFileEventSource(this IServiceCollection services,
            Action<EventSourceInstanceOptions> configureInstance = null)
        {
            services.AddEventSource<FileSystemEventSource>(configureInstance, typeof(FileSystemEventSourceConfiguration));

            return services;
        } 
        
        public static IEventFrameworkBuilder AddFileCloudEventSource(this IEventFrameworkBuilder builder,
            Action<EventSourceInstanceOptions> configureInstance = null)
        {
            var services = builder.Services;

            services.AddFileCloudEventSource(configureInstance);

            return builder;
        } 
        
        public static IServiceCollection AddFileCloudEventSource(this IServiceCollection services,
            Action<EventSourceInstanceOptions> configureInstance = null)
        {
            services.AddEventSource<FileCloudEventSource>(configureInstance, typeof(FileCloudEventSourceConfiguration));

            return services;
        } 
    }
}
