using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.EventSource
{
    public class EventSourceProvider : List<IEventSourceCatalog>
    {
        private readonly ILogger<EventSourceProvider> _logger;

        public EventSourceProvider(IEnumerable<IEventSourceCatalog> catalogs, ILogger<EventSourceProvider> logger)
        {
            _logger = logger;
            AddRange(catalogs);
        }
        
        public async Task Initialize(CancellationToken cancellationToken)
        {
            foreach (var catalog in this)
            {
                await catalog.Initialize(cancellationToken);
            }
        }

        public List<EventSourceDefinition> List()
        {
            var result = new List<EventSourceDefinition>();

            foreach (var catalog in this)
            {
                var definitionsInCatalog = catalog.List();
                result.AddRange(definitionsInCatalog);
            }

            return result;
        }

        public EventSource Get(EventSourceDefinition definition)
        {
            foreach (var catalog in this)
            {
                var eventSource = catalog.Get(definition);

                if (eventSource == null)
                {
                    continue;
                }

                return eventSource;
            }

            return null;
        }

        public EventSource Get(string name, Version version)
        {
            return Get(new EventSourceDefinition(name, version));
        }

        public EventSource Get(string name)
        {
            return Get(name, Version.Parse("1.0.0.0"));
        }
    }
}
