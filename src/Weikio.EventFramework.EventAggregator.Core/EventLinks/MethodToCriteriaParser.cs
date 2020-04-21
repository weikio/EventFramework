using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Weikio.EventFramework.EventAggregator.Core.EventLinks
{
    public class MethodToCriteriaParser
    {
        public static (CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard) MethodToCriteria(MethodInfo handlerMethod,
            List<MethodInfo> guardMethods)
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

            return (criteria, handlerMethod, guardMethod);
        }
    }
}
