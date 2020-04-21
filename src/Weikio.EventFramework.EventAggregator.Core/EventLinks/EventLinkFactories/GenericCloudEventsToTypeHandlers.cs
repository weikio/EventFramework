using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CloudNative.CloudEvents;
using Newtonsoft.Json;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventAggregator.Core.EventLinks.EventLinkFactories
{
    public class GenericCloudEventsToTypeHandlers : ITypeToHandlers
    {
        public int Priority { get; } = 0;

        public (List<(CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard)>, Func<MethodInfo, CloudEvent, List<object>>) GetHandlerMethods(Type handlerType)
        {
            var methods = handlerType.GetTypeInfo().DeclaredMethods.ToList();
            var handlerMethods = methods.Where(x =>
                !x.Name.StartsWith("Can") && x.GetParameters()
                    .Any(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(CloudEvent<>))).ToList();
            var guardMethods = methods.Where(x => x.Name.StartsWith("Can") && x.GetParameters().Any(p => p.ParameterType == typeof(CloudEvent))).ToList();

            var supportedCloudEventTypes = new List<(CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard)>();

            foreach (var handlerMethod in handlerMethods)
            {
                var methodCriteria = MethodToCriteriaParser.MethodToCriteria(handlerMethod, guardMethods);
                supportedCloudEventTypes.Add(methodCriteria);
            }

            return (supportedCloudEventTypes, GetArguments);
        }

        private static List<object> GetArguments(MethodInfo handler, CloudEvent cloudEvent)
        {
            var result = new List<object>()
            {
                ConvertCloudEventDataToGeneric(cloudEvent, handler)
            };

            foreach (var parameterInfo in handler.GetParameters())
            {
                if (!parameterInfo.HasDefaultValue)
                {
                    continue;
                }

                result.Add(parameterInfo.DefaultValue);
            }

            return result;
        }
        
        
        private static object ConvertCloudEventDataToGeneric(CloudEvent cloudEvent, MethodInfo handler)
        {
            var method = handler;

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
    }
}
