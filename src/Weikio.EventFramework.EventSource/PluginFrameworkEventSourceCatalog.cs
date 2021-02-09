using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class PluginFrameworkEventSourceCatalog : List<EventSource>, IEventSourceCatalog
    {
        private readonly IPluginCatalog _pluginCatalog;

        public PluginFrameworkEventSourceCatalog(IPluginCatalog pluginCatalog)
        {
            _pluginCatalog = pluginCatalog;
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            await _pluginCatalog.Initialize();
        }

        public List<EventSourceDefinition> List()
        {
            return this.Select(x => x.EventSourceDefinition).ToList();
        }

        public EventSource Get(EventSourceDefinition definition)
        {
            return GetEventSourceByDefinition(definition);
        }

        public EventSource Get(string name, Version version)
        {
            return Get(new EventSourceDefinition(name, version));
        }

        public EventSource Get(string name)
        {
            return Get(name, Version.Parse("1.0.0.0"));
        }

        private EventSource GetEventSourceByDefinition(EventSourceDefinition definition)
        {
            var plugin =  _pluginCatalog.Get(definition.Name, definition.Version);

            if (plugin == null)
            {
                return null;
            }

            var result = new EventSource(definition, eventSourceType: plugin.Type);

            return result;
        }
    }
}
