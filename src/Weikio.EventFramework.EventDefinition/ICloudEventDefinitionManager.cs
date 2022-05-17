using System.Collections.Generic;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventDefinition
{
    public interface ICloudEventDefinitionManager
    {
        void AddOrUpdate(CloudEventDefinition definition);
        bool TryAdd(CloudEventDefinition definition);
        IEnumerable<CloudEventDefinition> List();
        bool TryRemove(CloudEventDefinition definition);
    }
}
