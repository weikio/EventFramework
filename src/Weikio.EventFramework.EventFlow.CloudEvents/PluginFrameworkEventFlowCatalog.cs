using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weikio.EventFramework.EventFlow.Abstractions;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class PluginFrameworkEventFlowCatalog : List<EventFlowDefinition>, IEventFlowCatalog
    {
        private readonly IPluginCatalog _pluginCatalog;

        public PluginFrameworkEventFlowCatalog(IPluginCatalog pluginCatalog)
        {
            _pluginCatalog = pluginCatalog;
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            await _pluginCatalog.Initialize();
        }

        public List<EventFlowDefinition> List()
        {
            var result = new List<EventFlowDefinition>();

            var plugin = _pluginCatalog.Single();

            result.Add((plugin.Name, plugin.Version));

            return result;
        }

        public Type Get(EventFlowDefinition definition)
        {
            return GetByDefinition(definition);
        }

        private Type GetByDefinition(EventFlowDefinition definition)
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