using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventDefinition
{
    public class DefaultCloudEventDefinitionManager : ICloudEventDefinitionManager
    {
        private readonly ILogger<DefaultCloudEventDefinitionManager> _logger;
        private readonly ConcurrentDictionary<int, CloudEventDefinition> _definitions = new ConcurrentDictionary<int, CloudEventDefinition>();

        public DefaultCloudEventDefinitionManager(IEnumerable<CloudEventDefinition> initialDefinitions, ILogger<DefaultCloudEventDefinitionManager> logger)
        {
            _logger = logger;

            foreach (var cloudEventDefinition in initialDefinitions)
            {
                _definitions.TryAdd(cloudEventDefinition.GetHashCode(), cloudEventDefinition);
            }
        }

        public void AddOrUpdate(CloudEventDefinition definition)
        {
            _definitions[definition.GetHashCode()] = definition;
        }

        public bool TryAdd(CloudEventDefinition definition)
        {
            return _definitions.TryAdd(definition.GetHashCode(), definition);
        }
        
        public bool TryRemove(CloudEventDefinition definition)
        {
            return _definitions.TryRemove(definition.GetHashCode(), out definition);
        }

        public IEnumerable<CloudEventDefinition> List()
        {
            return _definitions.Select(x => x.Value);
        }
    }
}
