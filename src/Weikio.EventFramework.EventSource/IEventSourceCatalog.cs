using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventSource
{
    public interface IEventSourceCatalog
    {
        Task Initialize(CancellationToken cancellationToken);
        List<EventSourceDefinition> List();
        EventSource Get(EventSourceDefinition definition);
        EventSource Get(string name, Version version);
        EventSource Get(string name);
    }
}