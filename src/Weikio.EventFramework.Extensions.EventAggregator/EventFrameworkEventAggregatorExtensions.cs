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
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventAggregator.Core;

namespace Weikio.EventFramework.Extensions.EventAggregator
{
    public static class EventFrameworkEventAggregatorExtensions
    {
        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Action<CloudEvent> handler)
        {
            builder.Services.AddHandler(handler);

            return builder;
        }
        
        public static IServiceCollection AddHandler(this IServiceCollection services, Action<CloudEvent> handler)
        {
            return services.AddHandler(handler, string.Empty);
        }
        
        public static IServiceCollection AddHandler(this IServiceCollection services, Action<CloudEvent> handler, string eventType)
        {
            var func = new Func<CloudEvent, Task>(ev =>
            {
                handler(ev);

                return Task.CompletedTask;
            });

            return services.AddHandler(func, eventType, string.Empty);
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handler, string eventType)
        {
            builder.Services.AddHandler(handler, eventType);

            return builder;
        }
        
        public static IServiceCollection AddHandler(this IServiceCollection services, Func<CloudEvent, Task> handler, string eventType)
        {
            return services.AddHandler(handler, eventType, string.Empty);
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handler, string eventType, string source)
        {
            builder.Services.AddHandler(handler, eventType, source);
            return builder;
        }

        public static IServiceCollection AddHandler(this IServiceCollection services, Func<CloudEvent, Task> handler, string eventType, string source)
        {
            return services.AddHandler(handler, eventType, source, string.Empty);
        }
        
        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handler, string eventType, string source,
            string subject)
        {
            builder.Services.AddHandler(handler, eventType, source, subject);
            return builder;
        }
        
        public static IServiceCollection AddHandler(this IServiceCollection services, Func<CloudEvent, Task> handler, string eventType, string source,
            string subject)
        {
            var criteria = new CloudEventCriteria() { Type = eventType, Source = source, Subject = subject };

            return services.AddHandler(handler, criteria);
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handler, CloudEventCriteria criteria = null)
        {
             builder.Services.AddHandler(handler, criteria);
             return builder;
        }
        
        public static IServiceCollection AddHandler(this IServiceCollection services, Func<CloudEvent, Task> handler, CloudEventCriteria criteria = null)
        {
            if (criteria == null)
            {
                criteria = new CloudEventCriteria();
            }

            return services.AddHandler(handler, cloudEvent => criteria.CanHandle(cloudEvent));
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handle, Predicate<CloudEvent> canHandle)
        {
             builder.Services.AddHandler(handle, canHandle);
             return builder;
        }

        public static IServiceCollection AddHandler(this IServiceCollection services, Func<CloudEvent, Task> handle, Predicate<CloudEvent> canHandle)
        {
            return services.AddHandler(handle, cloudEvent => Task.FromResult(canHandle(cloudEvent)));
        }
        
        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handle,
            Func<CloudEvent, Task<bool>> canHandle)
        {
            builder.Services.AddHandler(handle, canHandle);

            return builder;
        }
        
        public static IServiceCollection AddHandler(this IServiceCollection services, Func<CloudEvent, Task> handle,
            Func<CloudEvent, Task<bool>> canHandle)
        {
            if (handle == null)
            {
                throw new ArgumentNullException(nameof(handle));
            }

            if (canHandle == null)
            {
                canHandle = cloudEvent => Task.FromResult(true);
            }

            services.AddTransient(provider =>
            {
                var result = new EventLink(canHandle, handle);

                return result;
            });

            return services;
        }

        public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, Action<THandlerType> configure = null)
            where THandlerType : class
        {
            return builder.AddHandler(typeof(THandlerType), cloudEvent => Task.FromResult(true), configure);
        }

        public static IServiceCollection AddHandler<THandlerType>(this IServiceCollection services, Action<THandlerType> configure = null)
            where THandlerType : class
        {
            return services.AddHandler(typeof(THandlerType), cloudEvent => Task.FromResult(true), configure);
        }

        public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, CloudEventCriteria criteria,
            Action<THandlerType> configure = null)
            where THandlerType : class
        {
            return builder.AddHandler(typeof(THandlerType), cloudEvent => Task.FromResult(criteria.CanHandle(cloudEvent)), configure);
        }

        public static IServiceCollection AddHandler<THandlerType>(this IServiceCollection services, CloudEventCriteria criteria,
            Action<THandlerType> configure = null)
            where THandlerType : class
        {
            return services.AddHandler(typeof(THandlerType), cloudEvent => Task.FromResult(criteria.CanHandle(cloudEvent)), configure);
        }

        public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, Func<CloudEvent, Task<bool>> canHandle,
            Action<THandlerType> configure = null)
            where THandlerType : class
        {
            return builder.AddHandler(typeof(THandlerType), canHandle, configure);
        }

        public static IServiceCollection AddHandler<THandlerType>(this IServiceCollection services, Func<CloudEvent, Task<bool>> canHandle,
            Action<THandlerType> configure = null)
            where THandlerType : class
        {
            return services.AddHandler(typeof(THandlerType), canHandle, configure);
        }

        public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, Predicate<CloudEvent> canHandle,
            Action<THandlerType> configure = null)
            where THandlerType : class
        {
            return builder.AddHandler(typeof(THandlerType), cloudEvent => Task.FromResult(canHandle(cloudEvent)), configure);
        }

        public static IServiceCollection AddHandler<THandlerType>(this IServiceCollection services, Predicate<CloudEvent> canHandle,
            Action<THandlerType> configure = null)
            where THandlerType : class
        {
            return services.AddHandler(typeof(THandlerType), cloudEvent => Task.FromResult(canHandle(cloudEvent)), configure);
        }

        public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, string eventType,
            Action<THandlerType> configure = null) where THandlerType : class
        {
            return builder.AddHandler<THandlerType>(eventType, string.Empty, configure);
        }

        public static IServiceCollection AddHandler<THandlerType>(this IServiceCollection services, string eventType,
            Action<THandlerType> configure = null) where THandlerType : class
        {
            return services.AddHandler<THandlerType>(eventType, string.Empty, configure);
        }

        public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, string eventType, string source,
            Action<THandlerType> configure = null) where THandlerType : class
        {
            return builder.AddHandler<THandlerType>(eventType, source, string.Empty, configure);
        }

        public static IServiceCollection AddHandler<THandlerType>(this IServiceCollection services, string eventType, string source,
            Action<THandlerType> configure = null) where THandlerType : class
        {
            return services.AddHandler<THandlerType>(eventType, source, string.Empty, configure);
        }

        public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, string eventType, string source,
            string subject, Action<THandlerType> configure = null) where THandlerType : class
        {
            var criteria = new CloudEventCriteria() { Type = eventType, Source = source, Subject = subject };

            return builder.AddHandler<THandlerType>(criteria, configure);
        }

        public static IServiceCollection AddHandler<THandlerType>(this IServiceCollection services, string eventType, string source,
            string subject, Action<THandlerType> configure = null) where THandlerType : class
        {
            var criteria = new CloudEventCriteria() { Type = eventType, Source = source, Subject = subject };

            return services.AddHandler<THandlerType>(criteria, configure);
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Type handlerType, Func<CloudEvent, Task<bool>> canHandle = null,
            MulticastDelegate configure = null)
        {
            builder.Services.AddHandler(handlerType, canHandle, configure);

            return builder;
        }

        public static IServiceCollection AddHandler(this IServiceCollection services, Type handlerType, Func<CloudEvent, Task<bool>> canHandle = null,
            MulticastDelegate configure = null)
        {
            services.TryAddTransient(handlerType);

            if (canHandle == null)
            {
                canHandle = ev => Task.FromResult(true);
            }
            
            services.AddSingleton(provider =>
            {
                var typeToEventLinksConverter = provider.GetRequiredService<ITypeToEventLinksConverter>();

                List<EventLink> Factory()
                {
                    var links = typeToEventLinksConverter.Create(provider, handlerType, canHandle, configure);

                    return links;
                }

                var result = new EventLinkSource(Factory);

                return result;
            });

            return services;
        }
    }
}
