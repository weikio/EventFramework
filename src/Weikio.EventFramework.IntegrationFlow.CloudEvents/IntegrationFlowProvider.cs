using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class IntegrationFlowDefinitionProvider : List<IntegrationFlowDefinition>
    {
        private readonly IEnumerable<IIntegrationFlowCatalog> _catalogs;
        private readonly ILogger<IntegrationFlowDefinitionProvider> _logger;
        private readonly IServiceProvider _serviceProvider;

        public IntegrationFlowDefinitionProvider(IEnumerable<IIntegrationFlowCatalog> catalogs, ILogger<IntegrationFlowDefinitionProvider> logger,
            IServiceProvider serviceProvider, IEnumerable<IntegrationFlowDefinition> definitions)
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
        
        public IntegrationFlowDefinition Get(IntegrationFlowDefinition definition)
        {
            foreach (var flowDefinition in this)
            {
                if (flowDefinition.Equals(definition))
                {
                    return flowDefinition;
                }
            }

            throw new UnknownIntegrationFlowException(
                $"No integration flow definition found with {definition}. Available definitions:{Environment.NewLine}{string.Join(Environment.NewLine, this.Select(x => x))}");
        }
    }

    public class IntegrationFlowProvider : List<IIntegrationFlowCatalog>
    {
        private readonly ILogger<IntegrationFlowProvider> _logger;
        private readonly IEnumerable<MyFlow> _flows;
        private readonly IServiceProvider _serviceProvider;

        public IntegrationFlowProvider(IEnumerable<IIntegrationFlowCatalog> catalogs, ILogger<IntegrationFlowProvider> logger, IEnumerable<MyFlow> flows,
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

        public List<IntegrationFlowDefinition> List()
        {
            var result = new List<IntegrationFlowDefinition>();

            foreach (var catalog in _flows)
            {
                result.Add(catalog.Definition);
            }

            return result;
        }

        public IntegrationFlowInstanceFactory Get(IntegrationFlowDefinition definition)
        {
            foreach (var catalog in _flows)
            {
                if (catalog.Definition.Equals(definition))
                {
                    return catalog.InstanceFactory;
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
