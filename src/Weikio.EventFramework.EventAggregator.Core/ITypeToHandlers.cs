using System;
using System.Collections.Generic;
using System.Reflection;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventAggregator.Core
{
    public interface ITypeToHandlers
    {
        int Priority { get; }
        (List<(CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard)>, Func<MethodInfo, CloudEvent, List<object>>) GetHandlerMethods(Type type);
    }
}
