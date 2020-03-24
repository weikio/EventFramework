using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventAggregator;
using Weikio.EventFramework.EventLinks.EventLinkFactories;

namespace Weikio.EventFramework.AspNetCore.Extensions
{
    public static class EventFrameworkEventSourceExtensions
    {
        // public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handler, string eventType)
        // {
        //     return builder.AddHandler(handler, eventType, string.Empty);
        // }
        //
        // public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handler, string eventType, string source)
        // {
        //     return builder.AddHandler(handler, eventType, source, string.Empty);
        // }
        //
        // public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handler, string eventType, string source,
        //     string subject)
        // {
        //     var criteria = new CloudEventCriteria() { Type = eventType, Source = source, Subject = subject };
        //
        //     return builder.AddHandler(handler, criteria);
        // }
        //
        // public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handler, CloudEventCriteria criteria = null)
        // {
        //     if (criteria == null)
        //     {
        //         criteria = new CloudEventCriteria();
        //     }
        //
        //     return builder.AddHandler(handler, cloudEvent => criteria.CanHandle(cloudEvent));
        // }
        //
        // public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handle, Predicate<CloudEvent> canHandle)
        // {
        //     return builder.AddHandler(handle, cloudEvent => Task.FromResult(canHandle(cloudEvent)));
        // }
        //
        // public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handle,
        //     Func<CloudEvent, Task<bool>> canHandle)
        // {
        //     if (handle == null)
        //     {
        //         throw new ArgumentNullException(nameof(handle));
        //     }
        //
        //     if (canHandle == null)
        //     {
        //         canHandle = cloudEvent => Task.FromResult(true);
        //     }
        //
        //     builder.Services.AddTransient(provider =>
        //     {
        //         var result = new EventLink(canHandle, handle);
        //
        //         return result;
        //     });
        //
        //     return builder;
        // }
        //
        // public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, Action<THandlerType> configure = null)
        //     where THandlerType : class
        // {
        //     return builder.AddHandler(typeof(THandlerType), cloudEvent => Task.FromResult(true), configure);
        // }
        //
        // public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, CloudEventCriteria criteria,
        //     Action<THandlerType> configure = null)
        //     where THandlerType : class
        // {
        //     return builder.AddHandler(typeof(THandlerType), cloudEvent => Task.FromResult(criteria.CanHandle(cloudEvent)), configure);
        // }
        //
        // public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, Func<CloudEvent, Task<bool>> canHandle,
        //     Action<THandlerType> configure = null)
        //     where THandlerType : class
        // {
        //     return builder.AddHandler(typeof(THandlerType), canHandle, configure);
        // }
        //
        // public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, Predicate<CloudEvent> canHandle,
        //     Action<THandlerType> configure = null)
        //     where THandlerType : class
        // {
        //     return builder.AddHandler(typeof(THandlerType), cloudEvent => Task.FromResult(canHandle(cloudEvent)), configure);
        // }
        //
        // public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, string eventType,
        //     Action<THandlerType> configure = null) where THandlerType : class
        // {
        //     return builder.AddHandler<THandlerType>(eventType, string.Empty, configure);
        // }
        //
        // public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, string eventType, string source,
        //     Action<THandlerType> configure = null) where THandlerType : class
        // {
        //     return builder.AddHandler<THandlerType>(eventType, source, string.Empty, configure);
        // }
        //
        // public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, string eventType, string source,
        //     string subject, Action<THandlerType> configure = null) where THandlerType : class
        // {
        //     var criteria = new CloudEventCriteria() { Type = eventType, Source = source, Subject = subject };
        //
        //     return builder.AddHandler<THandlerType>(criteria, configure);
        // }

        public static IEventFrameworkBuilder AddSource(this IEventFrameworkBuilder builder, Type sourceType, MulticastDelegate configure = null)
        {
            builder.Services.TryAddTransient(sourceType);

            // builder.Services.AddTransient(provider =>
            // {
            //     var typeToEventLinksConverter = provider.GetRequiredService<ITypeToEventLinksConverter>();
            //     
            //     List<EventLink> Factory()
            //     {
            //         var links = typeToEventLinksConverter.Create(provider, sourceType, canHandle, configure);
            //
            //         return links;
            //     }
            //
            //     var result = new EventLinkSource(Factory);
            //
            //     return result;
            // });

            return builder;
        }
    }
}
