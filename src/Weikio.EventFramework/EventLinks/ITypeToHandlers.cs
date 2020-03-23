using System;
using System.Collections.Generic;
using System.Reflection;
using CloudNative.CloudEvents;
using Weikio.EventFramework.EventAggregator;

namespace Weikio.EventFramework.EventLinks
{
    public interface ITypeToHandlers
    {
        int Priority { get; }
        (List<(CloudEventCriteria Criteria, MethodInfo Handler, MethodInfo Guard)>, Func<MethodInfo, CloudEvent, List<object>>) GetHandlerMethods(Type type);
    }
}
