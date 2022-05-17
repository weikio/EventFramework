using System;
using System.Collections.Generic;
using System.Reflection;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public interface ITypeToEventSourceTypeProvider
    {
        (List<MethodInfo> PollingSources, List<MethodInfo> LongPollingSources) GetSourceTypes(Type type);
    }
}
