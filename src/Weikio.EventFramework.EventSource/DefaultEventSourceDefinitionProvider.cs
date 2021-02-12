using System;
using System.Collections.Generic;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class DefaultEventSourceDefinitionProvider : IEventSourceDefinitionProvider
    {
        private readonly IEventSourceProvider _sourceProvider;
        private readonly IEventSourceDefinitionConfigurationTypeProvider _eventSourceDefinitionConfigurationTypeProvider;

        public DefaultEventSourceDefinitionProvider(IEventSourceProvider sourceProvider, IEventSourceDefinitionConfigurationTypeProvider eventSourceDefinitionConfigurationTypeProvider)
        {
            _sourceProvider = sourceProvider;
            _eventSourceDefinitionConfigurationTypeProvider = eventSourceDefinitionConfigurationTypeProvider;
        }

        public EventSourceDefinition GetByType(Type type)
        {
            var all = _sourceProvider.List();

            foreach (var def in all)
            {
                var source = _sourceProvider.Get(def);

                if (source.EventSourceType == type)
                {
                    def.ConfigurationType = GetConfigurationType(def);
                    
                    return def;
                }
            }

            return null;
        }

        public List<EventSourceDefinition> List()
        {
            var result = _sourceProvider.List();

            foreach (var def in result)
            {
                def.ConfigurationType = GetConfigurationType(def);
            }

            return result;
        }

        public EventSourceDefinition Get(string name, Version version)
        {
            var all = _sourceProvider.List();

            foreach (var def in all)
            {
                var source = _sourceProvider.Get(def);

                if (source.EventSourceDefinition == (name, version))
                {
                    def.ConfigurationType = GetConfigurationType(def);

                    return def;
                }
            }

            return null;
        }

        private Type GetConfigurationType(EventSourceDefinition definition)
        {
            return _eventSourceDefinitionConfigurationTypeProvider.Get(definition);
        }
    }
}