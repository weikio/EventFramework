using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventAggregator.Core.EventLinks.EventLinkFactories
{
    // TODO: Webhook support
    public class CloudEventsToTypeHandlers : ITypeToHandlers
    {
        public int Priority { get; } = 0;

        public (List<(CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard, Func<CloudEvent, Task<bool>> CanHandle)>, Func<MethodInfo, CloudEvent, List<object>>)
            GetHandlerMethods(Type handlerType, Func<CloudEvent, Task<bool>> canHandle)
        {
            var methods = handlerType.GetTypeInfo().DeclaredMethods.ToList();
            var handlerMethods = methods.Where(x => !x.Name.StartsWith("Can") && x.GetParameters().Any(p => p.ParameterType == typeof(CloudEvent))).ToList();
            var guardMethods = methods.Where(x => x.Name.StartsWith("Can") && x.GetParameters().Any(p => p.ParameterType == typeof(CloudEvent))).ToList();

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
            var result = new List<object>() { cloudEvent };

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
    }
}
