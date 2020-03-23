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

namespace Weikio.EventFramework.AspNetCore.Extensions
{
    public static class EventFrameworkEventAggregatorExtensions
    {
        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handler, string eventType)
        {
            return builder.AddHandler(handler, eventType, string.Empty);
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handler, string eventType, string source)
        {
            return builder.AddHandler(handler, eventType, source, string.Empty);
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handler, string eventType, string source,
            string subject)
        {
            var criteria = new CloudEventCriteria() { Type = eventType, Source = source, Subject = subject };

            return builder.AddHandler(handler, criteria);
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handler, CloudEventCriteria criteria = null)
        {
            if (criteria == null)
            {
                criteria = new CloudEventCriteria();
            }

            return builder.AddHandler(handler, cloudEvent => criteria.CanHandle(cloudEvent));
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handle, Predicate<CloudEvent> canHandle)
        {
            return builder.AddHandler(handle, cloudEvent => Task.FromResult(canHandle(cloudEvent)));
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Func<CloudEvent, Task> handle,
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

            builder.Services.AddTransient(provider =>
            {
                var result = new EventLink { Action = handle, CanHandle = canHandle };

                return result;
            });

            return builder;
        }

        public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, Action<THandlerType> configure = null)
            where THandlerType : class
        {
            return builder.AddHandler(typeof(THandlerType), cloudEvent => Task.FromResult(true), configure);
        }
        
        public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, CloudEventCriteria criteria, Action<THandlerType> configure = null)
            where THandlerType : class
        {
            return builder.AddHandler(typeof(THandlerType), cloudEvent => Task.FromResult(criteria.CanHandle(cloudEvent)), configure);
        }
        
        public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, Func<CloudEvent, Task<bool>> canHandle, Action<THandlerType> configure = null)
            where THandlerType : class
        {
            return builder.AddHandler(typeof(THandlerType), canHandle, configure);
        }
        
        public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, Predicate<CloudEvent> canHandle, Action<THandlerType> configure = null)
            where THandlerType : class
        {
            return builder.AddHandler(typeof(THandlerType), cloudEvent => Task.FromResult(canHandle(cloudEvent)), configure);
        }
        
        public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder,string eventType, Action<THandlerType> configure = null) where THandlerType : class
        {
            return builder.AddHandler<THandlerType>(eventType, string.Empty, configure);
        }

        public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder,  string eventType, string source, Action<THandlerType> configure = null) where THandlerType : class
        {
            return builder.AddHandler<THandlerType>(eventType, source, string.Empty, configure);
        }

        public static IEventFrameworkBuilder AddHandler<THandlerType>(this IEventFrameworkBuilder builder, string eventType, string source,
            string subject, Action<THandlerType> configure = null) where THandlerType : class
        {
            var criteria = new CloudEventCriteria() { Type = eventType, Source = source, Subject = subject };

            return builder.AddHandler<THandlerType>(criteria, configure);
        }        

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Type handlerType, Func<CloudEvent, Task<bool>> canHandle, MulticastDelegate configure = null)
        {
            builder.Services.TryAddTransient(handlerType);

            var supportedCloudEventTypes = GetSupportedCloudEventTypes(handlerType);
            var supportedGenericCloudEventTypes = GetSupportedGenericCloudEventTypes(handlerType);
            var supportedPublicHandlers = GetSupportedPublicTasks(handlerType);

            foreach (var supportedCloudEventType in supportedCloudEventTypes)
            {
                builder.Services.AddTransient(provider =>
                {
                    var handler = provider.GetRequiredService(handlerType);

                    configure?.DynamicInvoke(handler);

                    async Task Handle(CloudEvent cloudEvent)
                    {
                        try
                        {
                            var arguments = new List<object> { cloudEvent };

                            foreach (var parameterInfo in supportedCloudEventType.Handler.GetParameters())
                            {
                                if (!parameterInfo.HasDefaultValue)
                                {
                                    continue;
                                }

                                arguments.Add(parameterInfo.DefaultValue);
                            }

                            var res = (Task) supportedCloudEventType.Handler.Invoke(handler, arguments.ToArray());
                            await res;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }

                    async Task<bool> CanHandle(CloudEvent cloudEvent)
                    {
                        try
                        {
                            if (!supportedCloudEventType.Criteria.CanHandle(cloudEvent))
                            {
                                return false;
                            }

                            if (canHandle != null)
                            {
                                var res = await canHandle.Invoke(cloudEvent);

                                if (res == false)
                                {
                                    return res;
                                }
                            }
                            
                            if (supportedCloudEventType.Guard != null)
                            {
                                var res = (Task<bool>) supportedCloudEventType.Guard.Invoke(handler, new[] { cloudEvent });
                                await res;

                                return res.Result;
                            }

                            return true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);

                            return false;
                        }
                    }

                    var result = new EventLink { CanHandle = CanHandle, Action = Handle };

                    return result;
                });
            }

            foreach (var supportedGenericCloudEventType in supportedGenericCloudEventTypes)
            {
                builder.Services.AddTransient(provider =>
                {
                    var handler = provider.GetRequiredService(handlerType);

                    configure?.DynamicInvoke(handler);

                    async Task Handle(CloudEvent cloudEvent)
                    {
                        try
                        {
                            var arguments = new List<object> { ConvertCloudEventDataToGeneric(cloudEvent, supportedGenericCloudEventType) };

                            foreach (var parameterInfo in supportedGenericCloudEventType.Handler.GetParameters())
                            {
                                if (!parameterInfo.HasDefaultValue)
                                {
                                    continue;
                                }

                                arguments.Add(parameterInfo.DefaultValue);
                            }

                            var res = (Task) supportedGenericCloudEventType.Handler.Invoke(handler, arguments.ToArray());
                            await res;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }

                    async Task<bool> CanHandle(CloudEvent cloudEvent)
                    {
                        try
                        {
                            if (!supportedGenericCloudEventType.Criteria.CanHandle(cloudEvent))
                            {
                                return false;
                            }

                            if (canHandle != null)
                            {
                                var res = await canHandle.Invoke(cloudEvent);

                                if (res == false)
                                {
                                    return res;
                                }
                            }
                            
                            if (supportedGenericCloudEventType.Guard != null)
                            {
                                var res = (Task<bool>) supportedGenericCloudEventType.Guard.Invoke(handler, new[] { cloudEvent });
                                await res;

                                return res.Result;
                            }

                            return true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);

                            return false;
                        }
                    }

                    var result = new EventLink { CanHandle = CanHandle, Action = Handle };

                    return result;
                });
            }

            foreach (var supportedPublicHandler in supportedPublicHandlers.Where(x => !supportedCloudEventTypes.Select(m => m.Handler).Contains(x.Handler))
                .Where(x => !supportedGenericCloudEventTypes.Select(m => m.Handler).Contains(x.Handler)))
            {
                builder.Services.AddTransient(provider =>
                {
                    var handler = provider.GetRequiredService(handlerType);

                    configure?.DynamicInvoke(handler);

                    async Task Handle(CloudEvent cloudEvent)
                    {
                        try
                        {
                            var arguments = new List<object>();

                            foreach (var parameterInfo in supportedPublicHandler.Handler.GetParameters())
                            {
                                if (!parameterInfo.HasDefaultValue)
                                {
                                    var arg = ConvertCloudEventDataToObject(cloudEvent, parameterInfo.ParameterType);
                                    arguments.Add(arg);

                                    continue;
                                }

                                arguments.Add(parameterInfo.DefaultValue);
                            }

                            var res = (Task) supportedPublicHandler.Handler.Invoke(handler, arguments.ToArray());
                            await res;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }

                    async Task<bool> CanHandle(CloudEvent cloudEvent)
                    {
                        try
                        {
                            if (!supportedPublicHandler.Criteria.CanHandle(cloudEvent))
                            {
                                return false;
                            }

                            if (canHandle != null)
                            {
                                var res = await canHandle.Invoke(cloudEvent);

                                if (res == false)
                                {
                                    return res;
                                }
                            }
                            
                            if (supportedPublicHandler.Guard != null)
                            {
                                var res = (Task<bool>) supportedPublicHandler.Guard.Invoke(handler, new[] { cloudEvent });
                                await res;

                                return res.Result;
                            }

                            return true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);

                            return false;
                        }
                    }

                    var result = new EventLink { CanHandle = CanHandle, Action = Handle };

                    return result;
                });
            }

            return builder;
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
