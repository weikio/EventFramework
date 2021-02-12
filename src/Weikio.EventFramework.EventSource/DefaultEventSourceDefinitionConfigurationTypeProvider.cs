using System;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class DefaultEventSourceDefinitionConfigurationTypeProvider : IEventSourceDefinitionConfigurationTypeProvider
    {
        private readonly IOptionsMonitor<EventSourceConfigurationOptions> _optionsMonitor;
        private readonly IEventSourceProvider _sourceProvider;

        public DefaultEventSourceDefinitionConfigurationTypeProvider(IOptionsMonitor<EventSourceConfigurationOptions> optionsMonitor, IEventSourceProvider sourceProvider)
        {
            _optionsMonitor = optionsMonitor;
            _sourceProvider = sourceProvider;
        }

        public Type Get(EventSourceDefinition eventSourceDefinition)
        {
            if (eventSourceDefinition == null)
            {
                throw new ArgumentNullException(nameof(eventSourceDefinition));
            }
            
            var source = _sourceProvider.Get(eventSourceDefinition);
            var eventSourceType = source.EventSourceType;

            var result = _optionsMonitor.Get(eventSourceType.FullName);

            return result.ConfigurationType;
        }

    }
}