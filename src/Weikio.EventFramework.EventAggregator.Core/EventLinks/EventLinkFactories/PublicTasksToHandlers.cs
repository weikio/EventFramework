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
    public class PublicTasksToHandlers : ITypeToHandlers
    {
        public int Priority { get; } = 0;

        public (List<(CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard)>, Func<MethodInfo, CloudEvent, List<object>>) GetHandlerMethods(Type handlerType)
        {
            var methods = handlerType.GetTypeInfo().DeclaredMethods.ToList();

            var handlerMethods = methods.Where(x => !x.Name.StartsWith("Can") && x.ReturnType == typeof(Task) && x.IsPublic && !x.IsStatic
                                                    && x.GetParameters().All(p => p.ParameterType != typeof(CloudEvent))
                                                    && !x.GetParameters().Any(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(CloudEvent<>)));
            
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
            var result = new List<object>();

            foreach (var parameterInfo in handler.GetParameters())
            {
                if (!parameterInfo.HasDefaultValue)
                {
                    var arg = ConvertCloudEventDataToObject(cloudEvent, parameterInfo.ParameterType);
                    result.Add(arg);

                    continue;
                }

                result.Add(parameterInfo.DefaultValue);
            }

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

            if (cloudEvent.Data.GetType() == parameterType)
            {
                return cloudEvent.Data;
            }
            
            var result = JsonConvert.DeserializeObject(cloudEvent.Data.ToString(), parameterType);

            return result;
        }
    }
}
