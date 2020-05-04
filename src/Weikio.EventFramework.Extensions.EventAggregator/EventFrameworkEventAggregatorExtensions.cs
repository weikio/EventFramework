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
            
            services.AddTransient(provider =>
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

        private static object ConvertCloudEventDataToGeneric(CloudEvent cloudEvent,
            (CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard) supportedGenericCloudEventType)
        {
            var method = supportedGenericCloudEventType.Handler;

            var cloudEventParameter = method.GetParameters()
                .FirstOrDefault(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(CloudEvent<>));

            if (cloudEventParameter == null)
            {
                throw new NotSupportedException($"Couldn't find generic cloud event argument from {method.DeclaringType?.Name}.{method.Name}");
            }

            var dataType = cloudEvent.DataContentType?.MediaType;
            var isJson = true;
            var isXml = false;

            if (string.IsNullOrWhiteSpace(dataType))
            {
                // By default expect to have json-content
            }
            else if (string.Equals(dataType, "text/xaml", StringComparison.InvariantCultureIgnoreCase))
            {
                isXml = true;
                isJson = false;
            }
            else if (string.Equals(dataType, "application/json", StringComparison.InvariantCultureIgnoreCase))
            {
                isJson = true;
            }
            else
            {
                isJson = false;
            }

            if (!isJson && !isXml)
            {
                throw new NotSupportedException($"Content type {dataType} is not supported. Event type: {cloudEvent?.Type}");
            }

            var cloudEventObjectType = cloudEventParameter.ParameterType.GetProperties().FirstOrDefault()?.PropertyType;

            if (cloudEventObjectType == null)
            {
                throw new NotSupportedException($"Content type {dataType} is not supported. Event type: {cloudEvent?.Type}");
            }

            var obj = JsonConvert.DeserializeObject(cloudEvent.Data.ToString(), cloudEventObjectType);

            var d1 = typeof(CloudEvent<>);
            var constructed = d1.MakeGenericType(new[] { cloudEventObjectType });

            var mi = constructed.GetMethod("Create");

            var result = mi.Invoke(null, new[] { obj, cloudEvent });

            return result;
        }

        private static object ConvertCloudEventDataToObject(CloudEvent cloudEvent, Type parameterType)
        {
            var dataType = cloudEvent.DataContentType?.MediaType;
            var isJson = true;
            var isXml = false;

            if (string.IsNullOrWhiteSpace(dataType))
            {
                // By default expect to have json-content
            }
            else if (string.Equals(dataType, "text/xaml", StringComparison.InvariantCultureIgnoreCase))
            {
                isXml = true;
                isJson = false;
            }
            else if (string.Equals(dataType, "application/json", StringComparison.InvariantCultureIgnoreCase))
            {
                isJson = true;
            }
            else
            {
                isJson = false;
            }

            if (!isJson && !isXml)
            {
                throw new NotSupportedException($"Content type {dataType} is not supported. Event type: {cloudEvent?.Type}");
            }

            var result = JsonConvert.DeserializeObject(cloudEvent.Data.ToString(), parameterType);

            return result;
        }

        private static List<(CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard)> GetSupportedCloudEventTypes(Type handlerType)
        {
            var methods = handlerType.GetTypeInfo().DeclaredMethods.ToList();
            var handlerMethods = methods.Where(x => !x.Name.StartsWith("Can") && x.GetParameters().Any(p => p.ParameterType == typeof(CloudEvent))).ToList();
            var guardMethods = methods.Where(x => x.Name.StartsWith("Can") && x.GetParameters().Any(p => p.ParameterType == typeof(CloudEvent))).ToList();

            var supportedCloudEventTypes = new List<(CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard)>();

            foreach (var handlerMethod in handlerMethods)
            {
                var supportedEventType = string.Empty;
                var supportedSource = string.Empty;
                var supportedSubject = string.Empty;

                var cloudEventTypeParameter = handlerMethod.GetParameters()
                    .FirstOrDefault(x =>
                        string.Equals(x.Name, "eventType", StringComparison.InvariantCultureIgnoreCase) && x.ParameterType == typeof(string));

                if (cloudEventTypeParameter != null)
                {
                    supportedEventType = cloudEventTypeParameter.DefaultValue as string;
                }

                var cloudEventSourceParameter = handlerMethod.GetParameters()
                    .FirstOrDefault(x => string.Equals(x.Name, "source", StringComparison.InvariantCultureIgnoreCase) && x.ParameterType == typeof(string));

                if (cloudEventSourceParameter != null)
                {
                    supportedSource = cloudEventSourceParameter.DefaultValue as string;
                }

                var cloudEventSubjectParameter = handlerMethod.GetParameters()
                    .FirstOrDefault(x =>
                        string.Equals(x.Name, "subject", StringComparison.InvariantCultureIgnoreCase) && x.ParameterType == typeof(string));

                if (cloudEventSubjectParameter != null)
                {
                    supportedSubject = cloudEventSubjectParameter.DefaultValue as string;
                }

                var criteria = new CloudEventCriteria() { Type = supportedEventType, Source = supportedSource, Subject = supportedSubject };

                var guardMethod = guardMethods.FirstOrDefault(x =>
                    string.Equals(x.Name, "Can" + handlerMethod.Name, StringComparison.InvariantCultureIgnoreCase));

                supportedCloudEventTypes.Add((criteria, handlerMethod, guardMethod));
            }

            return supportedCloudEventTypes;
        }

        private static List<(CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard)> GetSupportedGenericCloudEventTypes(Type handlerType)
        {
            var methods = handlerType.GetTypeInfo().DeclaredMethods.ToList();

            var handlerMethods = methods.Where(x =>
                !x.Name.StartsWith("Can") && x.GetParameters()
                    .Any(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(CloudEvent<>))).ToList();
            var guardMethods = methods.Where(x => x.Name.StartsWith("Can") && x.GetParameters().Any(p => p.ParameterType == typeof(CloudEvent))).ToList();

            var supportedCloudEventTypes = new List<(CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard)>();

            foreach (var handlerMethod in handlerMethods)
            {
                var supportedEventType = string.Empty;
                var supportedSource = string.Empty;
                var supportedSubject = string.Empty;

                var cloudEventTypeParameter = handlerMethod.GetParameters()
                    .FirstOrDefault(x =>
                        string.Equals(x.Name, "eventType", StringComparison.InvariantCultureIgnoreCase) && x.ParameterType == typeof(string));

                if (cloudEventTypeParameter != null)
                {
                    supportedEventType = cloudEventTypeParameter.DefaultValue as string;
                }

                var cloudEventSourceParameter = handlerMethod.GetParameters()
                    .FirstOrDefault(x => string.Equals(x.Name, "source", StringComparison.InvariantCultureIgnoreCase) && x.ParameterType == typeof(string));

                if (cloudEventSourceParameter != null)
                {
                    supportedSource = cloudEventSourceParameter.DefaultValue as string;
                }

                var cloudEventSubjectParameter = handlerMethod.GetParameters()
                    .FirstOrDefault(x =>
                        string.Equals(x.Name, "subject", StringComparison.InvariantCultureIgnoreCase) && x.ParameterType == typeof(string));

                if (cloudEventSubjectParameter != null)
                {
                    supportedSubject = cloudEventSubjectParameter.DefaultValue as string;
                }

                var criteria = new CloudEventCriteria() { Type = supportedEventType, Source = supportedSource, Subject = supportedSubject };

                var guardMethod = guardMethods.FirstOrDefault(x =>
                    string.Equals(x.Name, "Can" + handlerMethod.Name, StringComparison.InvariantCultureIgnoreCase));

                supportedCloudEventTypes.Add((criteria, handlerMethod, guardMethod));
            }

            return supportedCloudEventTypes;
        }

        private static List<(CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard)> GetSupportedPublicTasks(Type handlerType)
        {
            var methods = handlerType.GetTypeInfo().DeclaredMethods.ToList();
            var handlerMethods = methods.Where(x => !x.Name.StartsWith("Can") && x.ReturnType == typeof(Task) && x.IsPublic && !x.IsStatic).ToList();
            var guardMethods = methods.Where(x => x.Name.StartsWith("Can") && x.GetParameters().Any(p => p.ParameterType == typeof(CloudEvent))).ToList();

            var supportedCloudEventTypes = new List<(CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard)>();

            foreach (var handlerMethod in handlerMethods)
            {
                var supportedEventType = string.Empty;
                var supportedSource = string.Empty;
                var supportedSubject = string.Empty;

                var cloudEventTypeParameter = handlerMethod.GetParameters()
                    .FirstOrDefault(x =>
                        string.Equals(x.Name, "eventType", StringComparison.InvariantCultureIgnoreCase) && x.ParameterType == typeof(string));

                if (cloudEventTypeParameter != null)
                {
                    supportedEventType = cloudEventTypeParameter.DefaultValue as string;
                }

                var cloudEventSourceParameter = handlerMethod.GetParameters()
                    .FirstOrDefault(x => string.Equals(x.Name, "source", StringComparison.InvariantCultureIgnoreCase) && x.ParameterType == typeof(string));

                if (cloudEventSourceParameter != null)
                {
                    supportedSource = cloudEventSourceParameter.DefaultValue as string;
                }

                var cloudEventSubjectParameter = handlerMethod.GetParameters()
                    .FirstOrDefault(x =>
                        string.Equals(x.Name, "subject", StringComparison.InvariantCultureIgnoreCase) && x.ParameterType == typeof(string));

                if (cloudEventSubjectParameter != null)
                {
                    supportedSubject = cloudEventSubjectParameter.DefaultValue as string;
                }

                var criteria = new CloudEventCriteria() { Type = supportedEventType, Source = supportedSource, Subject = supportedSubject };

                var guardMethod = guardMethods.FirstOrDefault(x =>
                    string.Equals(x.Name, "Can" + handlerMethod.Name, StringComparison.InvariantCultureIgnoreCase));

                supportedCloudEventTypes.Add((criteria, handlerMethod, guardMethod));
            }

            return supportedCloudEventTypes;
        }
    }
}
