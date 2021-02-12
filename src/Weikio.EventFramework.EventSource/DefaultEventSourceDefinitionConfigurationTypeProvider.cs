using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class DefaultEventSourceDefinitionConfigurationTypeProvider : IEventSourceDefinitionConfigurationTypeProvider
    {
        private readonly IOptionsMonitor<EventSourceConfigurationOptions> _optionsMonitor;
        private readonly IEventSourceProvider _sourceProvider;
        private readonly ConcurrentDictionary<string, Type> _cache = new ConcurrentDictionary<string, Type>();

        public DefaultEventSourceDefinitionConfigurationTypeProvider(IOptionsMonitor<EventSourceConfigurationOptions> optionsMonitor,
            IEventSourceProvider sourceProvider)
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

            var key = eventSourceDefinition.Name + eventSourceDefinition.Version;

            var result = _cache.GetOrAdd(key, s =>
            {
                var source = _sourceProvider.Get(eventSourceDefinition);
                var eventSourceType = source.EventSourceType;

                return _optionsMonitor.Get(eventSourceType.FullName).ConfigurationType;
            });

            return result;
        }
    }
}
