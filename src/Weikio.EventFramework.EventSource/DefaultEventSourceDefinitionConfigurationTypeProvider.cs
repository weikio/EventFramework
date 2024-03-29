﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.EventSource.Api;
using Weikio.EventFramework.EventSource.EventSourceWrapping;

namespace Weikio.EventFramework.EventSource
{
    public class DefaultEventSourceDefinitionConfigurationTypeProvider : IEventSourceDefinitionConfigurationTypeProvider
    {
        private readonly IOptionsMonitor<EventSourceConfigurationOptions> _optionsMonitor;
        private readonly IEventSourceProvider _sourceProvider;
        private readonly ITypeToEventSourceTypeProvider _typeToEventSourceTypeProvider;
        private readonly ConcurrentDictionary<string, EventSourceConfigurationType> _cache = new ConcurrentDictionary<string, EventSourceConfigurationType>();

        public DefaultEventSourceDefinitionConfigurationTypeProvider(IOptionsMonitor<EventSourceConfigurationOptions> optionsMonitor,
            IEventSourceProvider sourceProvider, ITypeToEventSourceTypeProvider typeToEventSourceTypeProvider)
        {
            _optionsMonitor = optionsMonitor;
            _sourceProvider = sourceProvider;
            _typeToEventSourceTypeProvider = typeToEventSourceTypeProvider;
        }

        public EventSourceConfigurationType Get(EventSourceDefinition eventSourceDefinition)
        {
            if (eventSourceDefinition == null)
            {
                throw new ArgumentNullException(nameof(eventSourceDefinition));
            }

            var source = _sourceProvider.Get(eventSourceDefinition);
            var eventSourceType = source.EventSourceType;

            return Get(eventSourceType);
        }

        public EventSourceConfigurationType Get(Type eventSourceType)
        {
            var key = eventSourceType.FullName;

            var result = _cache.GetOrAdd(key, s =>
            {
                var isHostedService = typeof(IHostedService).IsAssignableFrom(eventSourceType);
                var isApi = typeof(IApiEventSource).IsAssignableFrom(eventSourceType);

                var requiresPolling = (isHostedService || isApi) == false;

                if (!isHostedService && !isApi)
                {
                    var eventSourceTypes = _typeToEventSourceTypeProvider.GetSourceTypes(eventSourceType);

                    if (eventSourceTypes.PollingSources?.Any() != true)
                    {
                        requiresPolling = false;
                    }
                }

                var configurationTypeResult = _optionsMonitor.Get(eventSourceType.FullName).ConfigurationType;

                if (configurationTypeResult != null)
                {
                    return new EventSourceConfigurationType(requiresPolling, configurationTypeResult);
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

                if (configurationTypeResult != null)
                {
                    return new EventSourceConfigurationType(requiresPolling, configurationTypeResult);
                }

                if (isApi == false)
                {
                    return new EventSourceConfigurationType(requiresPolling, null);
                }

                // All Api Framework based event sources should be of type IApiEventSource<TConfigurationType>
                var apiConfigurationType = eventSourceType
                    .GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IApiEventSource<>))
                    .SelectMany(i => i.GetGenericArguments())
                    .FirstOrDefault();
                
                return new EventSourceConfigurationType(false, apiConfigurationType);
            });

            return result;
        }
    }
}
