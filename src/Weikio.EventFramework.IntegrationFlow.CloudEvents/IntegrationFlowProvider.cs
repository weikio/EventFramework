using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class IntegrationFlowProvider : List<IIntegrationFlowCatalog>
    {
        private readonly ILogger<IntegrationFlowProvider> _logger;

        public IntegrationFlowProvider(IEnumerable<IIntegrationFlowCatalog> catalogs, ILogger<IntegrationFlowProvider> logger)
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

        public List<IntegrationFlowDefinition> List()
        {
            var result = new List<IntegrationFlowDefinition>();

            foreach (var catalog in this)
            {
                var definitionsInCatalog = catalog.List();
                result.AddRange(definitionsInCatalog);
            }

            return result;
        }

        public Type Get(IntegrationFlowDefinition definition)
        {
            foreach (var catalog in this)
            {
                var integrationFlow = catalog.Get(definition);

                if (integrationFlow == null)
                {
                    continue;
                }

                return integrationFlow;
            }

            var allDefinitions = List();

            throw new UnknownIntegrationFlowException(
                $"No integration flow found with definition {definition}. Available definitions:{Environment.NewLine}{string.Join(Environment.NewLine, allDefinitions)}");
        }


        public async Task AddCatalog(IIntegrationFlowCatalog integrationFlowCatalog)
        {
            await integrationFlowCatalog.Initialize(new CancellationToken());

            Add(integrationFlowCatalog);
        }
    }
}