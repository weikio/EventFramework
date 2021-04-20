using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Newtonsoft.Json;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventAggregator.Core.EventLinks.EventLinkFactories
{
    public class GenericCloudEventsToTypeHandlers : ITypeToHandlers
    {
        public int Priority { get; } = 0;

        public (List<(CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard, Func<CloudEvent, Task<bool>> CanHandle)>, Func<MethodInfo, CloudEvent, List<object>>)
            GetHandlerMethods(Type handlerType, Func<CloudEvent, Task<bool>> canHandle)
        {
            var methods = handlerType.GetTypeInfo().DeclaredMethods.ToList();

            var handlerMethods = methods.Where(x =>
                !x.Name.StartsWith("Can") && x.GetParameters()
                    .Any(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(CloudEvent<>))).ToList();
            var guardMethods = methods.Where(x => x.Name.StartsWith("Can") && x.GetParameters().Any(p => p.ParameterType == typeof(CloudEvent))).ToList();

            guardMethods.AddRange(methods.Where(x =>
                x.Name.StartsWith("Can") && x.GetParameters()
                    .Any(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(CloudEvent<>))).ToList());

            var supportedCloudEventTypes = new List<(CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard, Func<CloudEvent, Task<bool>> CanHandle)>();

            foreach (var handlerMethod in handlerMethods)
            {
                var methodCriteria = MethodToCriteriaParser.MethodToCriteria(handlerMethod, guardMethods, canHandle);
                supportedCloudEventTypes.Add(methodCriteria);
            }

            return (supportedCloudEventTypes, GetArguments);
        }

        private static List<object> GetArguments(MethodInfo handler, CloudEvent cloudEvent)
        {
            var parameters = handler.GetParameters();

            if (!parameters.Any())
            {
                return new List<object>();
            }

            if (parameters.Length == 1 && parameters.First().ParameterType == typeof(CloudEvent))
            {
                return new List<object>() { cloudEvent };
            }

            var result = new List<object>() { ConvertCloudEventDataToGeneric(cloudEvent, handler) };

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

            object obj;

            if (cloudEvent.Data.GetType() == cloudEventObjectType)
            {
                obj = cloudEvent.Data;
            }
            else
            {
                obj = JsonConvert.DeserializeObject(cloudEvent.Data.ToString(), cloudEventObjectType);
            }

            var genericCloudEvent = typeof(CloudEvent<>);
            var constructed = genericCloudEvent.MakeGenericType(new[] { cloudEventObjectType });

            var mi = constructed.GetMethods().FirstOrDefault(x => x.GetParameters().Length == 2);

            if (mi == null)
            {
                throw new Exception("Couldn't find Create method from generic CloudEvent");
            }

            var result = mi.Invoke(null, new[] { obj, cloudEvent });

            return result;
        }
    }
}
