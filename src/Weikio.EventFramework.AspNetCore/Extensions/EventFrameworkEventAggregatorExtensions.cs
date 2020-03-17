using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
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
            if (handle == null)
            {
                throw new ArgumentNullException(nameof(handle));
            }
            
            if (canHandle == null)
            {
                canHandle = cloudEvent => true;
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
            builder.Services.AddTransient<THandlerType>();

            var supportedCloudEventTypes = GetSupportedCloudEventTypes<THandlerType>();

            foreach (var supportedCloudEventType in supportedCloudEventTypes)
            {
                builder.Services.AddTransient(provider =>
                {
                    var handler = provider.GetRequiredService<THandlerType>();

                    configure?.Invoke(handler);

                    async Task HandlerAction(CloudEvent cloudEvent)
                    {
                        var arguments = new List<object> { cloudEvent };

                        foreach (var parameterInfo in supportedCloudEventType.Item2.GetParameters())
                        {
                            if (!parameterInfo.HasDefaultValue)
                            {
                                continue;
                            }

                            arguments.Add(parameterInfo.DefaultValue);
                        }

                        var res = (Task) supportedCloudEventType.Item2.Invoke(handler, arguments.ToArray());
                        await res;
                    }

                    var result = new EventLink
                    {
                        CanHandle = cloudEvent => supportedCloudEventType.Item1.CanHandle(cloudEvent),
                        Action = HandlerAction
                    };

                    return result;
                });
            }

            return builder;
        }

        private static List<Tuple<CloudEventCriteria, MethodInfo>> GetSupportedCloudEventTypes<THandlerType>() where THandlerType : class
        {
            var methods = typeof(THandlerType).GetTypeInfo().DeclaredMethods.ToList();
            var methodsWithCloudEvent = methods.Where(x => x.GetParameters().Any(p => p.ParameterType == typeof(CloudEvent))).ToList();

            var supportedCloudEventTypes = new List<Tuple<CloudEventCriteria, MethodInfo>>();

            foreach (var methodWithCloudEvent in methodsWithCloudEvent)
            {
                var supportedEventType = string.Empty;
                var supportedSource = string.Empty;
                var supportedSubject = string.Empty;

                var cloudEventTypeParameter = methodWithCloudEvent.GetParameters()
                    .FirstOrDefault(x =>
                        string.Equals(x.Name, "eventType", StringComparison.InvariantCultureIgnoreCase) && x.ParameterType == typeof(string));

                if (cloudEventTypeParameter != null)
                {
                    supportedEventType = cloudEventTypeParameter.DefaultValue as string;
                }

                var cloudEventSourceParameter = methodWithCloudEvent.GetParameters()
                    .FirstOrDefault(x => string.Equals(x.Name, "source", StringComparison.InvariantCultureIgnoreCase) && x.ParameterType == typeof(string));

                if (cloudEventSourceParameter != null)
                {
                    supportedSource = cloudEventSourceParameter.DefaultValue as string;
                }

                var cloudEventSubjectParameter = methodWithCloudEvent.GetParameters()
                    .FirstOrDefault(x =>
                        string.Equals(x.Name, "subject", StringComparison.InvariantCultureIgnoreCase) && x.ParameterType == typeof(string));

                if (cloudEventSubjectParameter != null)
                {
                    supportedSubject = cloudEventSubjectParameter.DefaultValue as string;
                }

                var criteria = new CloudEventCriteria() { Type = supportedEventType, Source = supportedSource, Subject = supportedSubject };

                supportedCloudEventTypes.Add(new Tuple<CloudEventCriteria, MethodInfo>(criteria, methodWithCloudEvent));
            }

            return supportedCloudEventTypes;
        }
    }
}
