using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class IntegrationFlowProvider : List<IIntegrationFlowCatalog>
    {
        private readonly ILogger<IntegrationFlowProvider> _logger;
        private readonly IEnumerable<MyFlow> _flows;

        public IntegrationFlowProvider(IEnumerable<IIntegrationFlowCatalog> catalogs, ILogger<IntegrationFlowProvider> logger, IEnumerable<MyFlow> flows)
        {
            _logger = logger;
            _flows = flows;
            AddRange(catalogs);
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            foreach (var catalog in this)
            {
                await catalog.Initialize(cancellationToken);
            }
        }

        public List<IntegrationFlowDefinition> List()
        {
            var result = new List<IntegrationFlowDefinition>();

            foreach (var catalog in _flows)
            {
                result.Add(catalog.Definition);
            }

            return result;
        }

        public CloudEventsIntegrationFlowFactory Get(IntegrationFlowDefinition definition)
        {
            foreach (var catalog in _flows)
            {
                if (catalog.Definition.Equals( definition))
                {
                    return catalog.Factory;
                }
            }

            throw new UnknownIntegrationFlowException(
                $"No integration flow found with definition {definition}. Available definitions:{Environment.NewLine}{string.Join(Environment.NewLine, _flows.Select(x => x.Definition.Name))}");
        }


        public async Task AddCatalog(IIntegrationFlowCatalog integrationFlowCatalog)
        {
            await integrationFlowCatalog.Initialize(new CancellationToken());

            Add(integrationFlowCatalog);
        }
    }
}
