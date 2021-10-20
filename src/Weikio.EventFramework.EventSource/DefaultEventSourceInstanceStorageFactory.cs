using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    internal class DefaultEventSourceInstanceStorageFactory : IEventSourceInstanceStorageFactory
    {
        private readonly IOptions<DefaultEventSourceInstanceStorageFactoryOptions> _options;
        private readonly IServiceProvider _serviceProvider;

        private readonly ConcurrentDictionary<string, IEventSourceInstanceDataStore> _dataStores =
            new ConcurrentDictionary<string, IEventSourceInstanceDataStore>();

        public DefaultEventSourceInstanceStorageFactory(IOptions<DefaultEventSourceInstanceStorageFactoryOptions> options, IServiceProvider serviceProvider)
        {
            _options = options;
            _serviceProvider = serviceProvider;
        }

        public Task<IEventSourceInstanceDataStore> GetStorage(EventSourceInstance eventSourceInstance, Type eventSourceInstanceStateType)
        {
            var result = _dataStores.GetOrAdd(eventSourceInstance.Id, s =>
            {
                // If persistence is configured to the instance, use that
                if (eventSourceInstance.Options.EventSourceInstanceDataStore != null)
                {
                    return eventSourceInstance.Options.EventSourceInstanceDataStore(_serviceProvider, eventSourceInstance, eventSourceInstanceStateType);
                }
                
                if (eventSourceInstance.Options.PersistState)
                {
                    var optionsValue = _options.Value;

                    return optionsValue.CreateDefaultPersistableEventSourceInstanceDataStore(_serviceProvider, eventSourceInstance, eventSourceInstanceStateType);
                }

                var inMemory = new InMemoryEventSourceInstanceDataStore { EventSourceInstance = eventSourceInstance, StateType = eventSourceInstanceStateType};

                return inMemory;
            });

            return Task.FromResult(result);
        }
    }
}
