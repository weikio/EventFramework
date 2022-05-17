using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class DefaultEventSourceFactory : IEventSourceFactory
    {
        public Abstractions.EventSource Create(EventSourceDefinition definition, MulticastDelegate action = null, Type eventSourceType = null, object eventSourceInstance = null)
        {
            var result = new Abstractions.EventSource(definition, action, eventSourceType, eventSourceInstance);

            return result;
        }
    }
}
