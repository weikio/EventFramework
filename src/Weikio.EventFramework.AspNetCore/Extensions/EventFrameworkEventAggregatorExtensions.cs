using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            return builder.AddHandler(typeof(THandlerType), configure);
        }

        public static IEventFrameworkBuilder AddHandler(this IEventFrameworkBuilder builder, Type handlerType, MulticastDelegate configure = null)
        {
            builder.Services.TryAddTransient(handlerType);

            var supportedCloudEventTypes = GetSupportedCloudEventTypes(handlerType);

            foreach (var supportedCloudEventType in supportedCloudEventTypes)
            {
                builder.Services.AddTransient(provider =>
                {
                    var handler = provider.GetRequiredService(handlerType);

                    configure?.DynamicInvoke(handler);
                    // configure?.Invoke(handler);

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

                            if (supportedCloudEventType.Guard == null)
                            {
                                return true;
                            }

                            var res = (Task<bool>) supportedCloudEventType.Guard.Invoke(handler, new[] { cloudEvent });
                            await res;

                            return res.Result;
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
    }
}
