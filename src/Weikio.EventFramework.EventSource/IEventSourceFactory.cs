using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public interface IEventSourceFactory
    {
        Abstractions.EventSource Create(EventSourceDefinition eventSourceDefinition, MulticastDelegate action = null, Type eventSourceType = null, object eventSourceInstance = null);
    }
}
