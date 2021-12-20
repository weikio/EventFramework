using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventFlow.Abstractions;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class EventFlowProvider : List<IEventFlowCatalog>
    {
        private readonly ILogger<EventFlowProvider> _logger;
        private readonly IEnumerable<MyFlow> _flows;
        private readonly IServiceProvider _serviceProvider;

        public EventFlowProvider(IEnumerable<IEventFlowCatalog> catalogs, ILogger<EventFlowProvider> logger, IEnumerable<MyFlow> flows,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _flows = flows;
            _serviceProvider = serviceProvider;

            AddRange(catalogs);
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            foreach (var catalog in this)
            {
                await catalog.Initialize(cancellationToken);
            }
        }

        public List<EventFlowDefinition> List()
        {
            var result = new List<EventFlowDefinition>();

            foreach (var catalog in _flows)
            {
                result.Add(catalog.Definition);
            }

            return result;
        }

        public EventFlowInstanceFactory Get(EventFlowDefinition definition)
        {
            foreach (var catalog in _flows)
            {
                if (catalog.Definition.Equals(definition))
                {
                    return catalog.InstanceFactory;
                }
            }

            throw new UnknownEventFlowException(
                $"No event flow found with definition {definition}. Available definitions:{Environment.NewLine}{string.Join(Environment.NewLine, _flows.Select(x => x.Definition.Name))}");
        }

        public async Task AddCatalog(IEventFlowCatalog eventFlowCatalog)
        {
            await eventFlowCatalog.Initialize(new CancellationToken());

            Add(eventFlowCatalog);
        }
    }
}
