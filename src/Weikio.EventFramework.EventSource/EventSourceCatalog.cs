using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class EventSourceCatalog : List<Abstractions.EventSource>, IEventSourceCatalog
    {
        public Task Initialize(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public List<EventSourceDefinition> List()
        {
            return this.Select(x => x.EventSourceDefinition).ToList();
        }

        public Abstractions.EventSource Get(EventSourceDefinition definition)
        {
            return this.FirstOrDefault(x => x.EventSourceDefinition == definition);
        }

        public Abstractions.EventSource Get(string name, Version version)
        {
            return Get(new EventSourceDefinition(name, version));
        }

        public Abstractions.EventSource Get(string name)
        {
            return Get(name, Version.Parse("1.0.0.0"));
        }
    }
}
