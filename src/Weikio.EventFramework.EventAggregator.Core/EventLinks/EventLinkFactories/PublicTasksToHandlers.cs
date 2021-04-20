using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventAggregator.Core.EventLinks.EventLinkFactories
{
    public class PublicTasksToHandlers : ITypeToHandlers
    {
        public int Priority { get; } = 0;

        public (List<(CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard, Func<CloudEvent, Task<bool>> CanHandle)>, Func<MethodInfo, CloudEvent, List<object>>) GetHandlerMethods(Type handlerType, Func<CloudEvent, Task<bool>> canHandle)
        {
            var methods = handlerType.GetTypeInfo().DeclaredMethods.ToList();

            var handlerMethods = methods.Where(x => !x.Name.StartsWith("Can") && x.ReturnType == typeof(Task) && x.IsPublic && !x.IsStatic
                                                    && x.GetParameters().All(p => p.ParameterType != typeof(CloudEvent))
                                                    && !x.GetParameters().Any(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(CloudEvent<>)));
            
            var guardMethods = methods.Where(x => x.Name.StartsWith("Can")).ToList();

            var supportedCloudEventTypes = new List<(CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard, Func<CloudEvent, Task<bool>> CanHandle)>();

            foreach (var handlerMethod in handlerMethods)
            {
                var methodCriteria = MethodToCriteriaParser.MethodToCriteria(handlerMethod, guardMethods, canHandle);

                // If developer doesn't add any criteria to handler, automatically add the handler's first parameter as a EventType filter
                if (methodCriteria.Criteria.Equals(CloudEventCriteria.Empty))
                {
                    
                    var methodParameters = handlerMethod.GetParameters();

                    if (methodParameters.Any())
                    {
                        var methodParameter = methodParameters.First();

                        methodCriteria.CanHandle = async ev =>
                        {
                            if (canHandle != null)
                            {
                                var canHandleResult = await canHandle(ev);

                                if (canHandleResult == false)
                                {
                                    return false;
                                }
                            }
                            
                            if (ev.Data.GetType() != methodParameter.ParameterType)
                            {
                                return false;
                            }

                            return true;
                        };

                        // Now we need a way to convert this method parameter to event type. 
                        // By default the conversion is easy: Type: CustomerCreatedEvent, EventType: CustomerCreatedEvent
                        // But the developer can configure how the event names 
                    }
                }
                
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
            
            var result = new List<object>();
            foreach (var parameterInfo in parameters)
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

            if (cloudEvent.Data is JToken)
            {
                return JsonConvert.DeserializeObject(cloudEvent.Data.ToString(), parameterType);
            }

            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(cloudEvent.Data), parameterType);
        }
    }
}
