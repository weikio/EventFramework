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

            var source = _sourceProvider.Get(eventSourceDefinition);
            var eventSourceType = source.EventSourceType;

            return Get(eventSourceType);
        }

        public Type Get(Type eventSourceType)
        {
            var key = eventSourceType.FullName;

            var result = _cache.GetOrAdd(key, s =>
            {
                var configurationTypeResult = _optionsMonitor.Get(eventSourceType.FullName).ConfigurationType;

                if (configurationTypeResult != null)
                {
                    return configurationTypeResult;
                }

                var ctors = eventSourceType.GetConstructors();

                foreach (var constructorInfo in ctors)
                {
                    foreach (var param in constructorInfo.GetParameters())
                    {
                        if (!string.Equals(param.Name, "configuration", StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }

                        if (param.ParameterType.Assembly != eventSourceType.Assembly)
                        {
                            continue;
                        }

                        configurationTypeResult = param.ParameterType;

                        break;
                    }

                    if (configurationTypeResult == null)
                    {
                        continue;
                    }

                    break;
                }

                return configurationTypeResult;
            });

            return result;
        }
    }
}
