using System;

namespace Weikio.EventFramework.EventSource.Abstractions
{
    public interface IEventSourceDefinitionConfigurationTypeProvider
    {
        EventSourceConfigurationType Get(EventSourceDefinition eventSourceDefinition);
        EventSourceConfigurationType Get(Type eventSourceType);
    }
}
