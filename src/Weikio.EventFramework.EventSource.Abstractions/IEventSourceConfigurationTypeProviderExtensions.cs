using System;

namespace Weikio.EventFramework.EventSource.Abstractions
{
    public static class IEventSourceConfigurationTypeProviderExtensions
    {
        public static Type Get(this IEventSourceDefinitionConfigurationTypeProvider definitionConfigurationTypeProvider, EventSource es)
        {
            return definitionConfigurationTypeProvider.Get(es.EventSourceDefinition);
        }
    }
}