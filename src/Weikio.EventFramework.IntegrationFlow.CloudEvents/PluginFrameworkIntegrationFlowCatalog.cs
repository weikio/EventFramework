using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class PluginFrameworkIntegrationFlowCatalog : List<IntegrationFlowDefinition>, IIntegrationFlowCatalog
    {
        private readonly IPluginCatalog _pluginCatalog;

        public PluginFrameworkIntegrationFlowCatalog(IPluginCatalog pluginCatalog)
        {
            _pluginCatalog = pluginCatalog;
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            await _pluginCatalog.Initialize();
        }

        public List<IntegrationFlowDefinition> List()
        {
            var result = new List<IntegrationFlowDefinition>();

            var plugin = _pluginCatalog.Single();

            result.Add((plugin.Name, plugin.Version));

            return result;
        }

        public Type Get(IntegrationFlowDefinition definition)
        {
            return GetByDefinition(definition);
        }

        private Type GetByDefinition(IntegrationFlowDefinition definition)
        {
            var plugin = _pluginCatalog.Get(definition.Name, definition.Version);
            Type nullType = null;
            if (plugin == null)
            {
                // Plugin Framework throws an error in implicit conversion, get around it
                // ReSharper disable once ExpressionIsAlwaysNull
                return nullType;
            }

            return plugin;
        }
    }
}