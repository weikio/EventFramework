using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.EventFramework.EventSource
{
    public class PluginEventSourceCatalog : List<PluginFrameworkEventSourceCatalog>, IEventSourceCatalog
    {
        public PluginEventSourceCatalog(IEnumerable<EventSourcePlugin> plugins)
        {
            foreach (var plugin in plugins)
            {
                var typePluginCatalog = new TypePluginCatalog(plugin.EventSourceType);
                var catalog = new PluginFrameworkEventSourceCatalog(typePluginCatalog);

                Add(catalog);
            }
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

            foreach (var pluginCatalog in this)
            {
                var all = pluginCatalog.List();
                result.AddRange(all);
            }

            return result;
        }

        public Abstractions.EventSource Get(EventSourceDefinition definition)
        {
            foreach (var catalog in this)
            {
                var result = catalog.Get(definition);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
