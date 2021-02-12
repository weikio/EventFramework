using System;

namespace Weikio.EventFramework.EventSource.Abstractions
{
    public interface IEventSourceDefinitionConfigurationTypeProvider
    {
        Type Get(EventSourceDefinition eventSourceDefinition);
    }
}