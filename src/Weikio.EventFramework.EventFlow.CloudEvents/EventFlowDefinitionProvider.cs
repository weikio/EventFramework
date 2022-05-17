using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventFlow.Abstractions;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class EventFlowDefinitionProvider : List<EventFlowDefinition>
    {
        private readonly IEnumerable<IEventFlowCatalog> _catalogs;
        private readonly ILogger<EventFlowDefinitionProvider> _logger;
        private readonly IServiceProvider _serviceProvider;

        public EventFlowDefinitionProvider(IEnumerable<IEventFlowCatalog> catalogs, ILogger<EventFlowDefinitionProvider> logger,
            IServiceProvider serviceProvider, IEnumerable<EventFlowDefinition> definitions)
        {
            _catalogs = catalogs;
            _logger = logger;
            _serviceProvider = serviceProvider;
            AddRange(definitions);
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            foreach (var catalog in _catalogs)
            {
                await catalog.Initialize(cancellationToken);

                var definitions = catalog.List();
                AddRange(definitions);
            }
        }
        
        public EventFlowDefinition Get(EventFlowDefinition definition)
        {
            foreach (var flowDefinition in this)
            {
                if (flowDefinition.Equals(definition))
                {
                    return flowDefinition;
                }
            }

            throw new UnknownEventFlowException(
                $"No integration flow definition found with {definition}. Available definitions:{Environment.NewLine}{string.Join(Environment.NewLine, this.Select(x => x))}");
        }
    }
}
