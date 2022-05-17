using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class DefaultEventSourceProvider : List<IEventSourceCatalog>, IEventSourceProvider
    {
        private readonly ILogger<DefaultEventSourceProvider> _logger;

        public DefaultEventSourceProvider(IEnumerable<IEventSourceCatalog> catalogs, ILogger<DefaultEventSourceProvider> logger)
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

        public Abstractions.EventSource Get(EventSourceDefinition definition)
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

            var allDefinitions = List();

            throw new UnknownEventSourceException(
                $"No event source found with definition {definition}. Available definitions:{Environment.NewLine}{string.Join(Environment.NewLine, allDefinitions)}");
        }

        public async Task AddCatalog(IEventSourceCatalog eventSourceCatalog)
        {
            await eventSourceCatalog.Initialize(new CancellationToken());
            
            Add(eventSourceCatalog);
        }
    }
}
