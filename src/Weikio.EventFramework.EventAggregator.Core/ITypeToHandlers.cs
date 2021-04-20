using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventAggregator.Core
{
    public interface ITypeToHandlers
    {
        int Priority { get; }
        (List<(CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard, Func<CloudEvent, Task<bool>> CanHandle)>, Func<MethodInfo, CloudEvent, List<object>>) GetHandlerMethods(Type type, Func<CloudEvent, Task<bool>> canHandle);
    }
}
