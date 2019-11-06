using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Extensions
{
    public static class EventFrameworkRouteExtensions
    {
        public static IEventFrameworkBuilder AddRoute(this IEventFrameworkBuilder builder, ICloudEventRoute route)
        {
            return builder;
        }

        public static IEventFrameworkBuilder AddRoute(this IEventFrameworkBuilder builder, string eventType, Func<CloudEvent, Task> handle)
        {
            return builder;
        }
        
        public static IEventFrameworkBuilder AddRoute(this IEventFrameworkBuilder builder, string eventType, Func<IServiceProvider, CloudEvent, Task> handle)
        {
            return builder;
        }

        public static IEventFrameworkBuilder AddRoute(this IEventFrameworkBuilder builder, Predicate<CloudEvent> canHandle, Func<CloudEvent, Task> handle)
        {
            return builder;
        }

        public static IEventFrameworkBuilder AddRoute(this IEventFrameworkBuilder builder, Predicate<CloudEvent> canHandle, Func<IServiceProvider, CloudEvent, Task> handle)
        {
            return builder;
        }
    }
}