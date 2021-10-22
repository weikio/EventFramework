using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    internal class DefaultEventSourceInstanceStorageFactory : IEventSourceInstanceStorageFactory
    {
        private readonly IOptions<DefaultEventSourceInstanceStorageFactoryOptions> _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DefaultEventSourceInstanceStorageFactory> _logger;

        private readonly ConcurrentDictionary<string, IEventSourceInstanceDataStore> _dataStores =
            new ConcurrentDictionary<string, IEventSourceInstanceDataStore>();

        public DefaultEventSourceInstanceStorageFactory(IOptions<DefaultEventSourceInstanceStorageFactoryOptions> options, IServiceProvider serviceProvider, ILogger<DefaultEventSourceInstanceStorageFactory> logger)
        {
            _options = options;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task<IEventSourceInstanceDataStore> GetStorage(EventSourceInstance eventSourceInstance, Type eventSourceInstanceStateType)
        {
            var result = _dataStores.GetOrAdd(eventSourceInstance.Id, s =>
            {
                _logger.LogDebug("Getting storage for Event Source Instance with Id {ID}", eventSourceInstance.Id);

                // If persistence is configured to the instance, use that
                if (eventSourceInstance.Options.EventSourceInstanceDataStore != null)
                {
                    _logger.LogDebug("Event Source Instance with Id {ID} has manually configured EventSourceInstanceDataStore, using that", eventSourceInstance.Id);

                    return eventSourceInstance.Options.EventSourceInstanceDataStore(_serviceProvider, eventSourceInstance, eventSourceInstanceStateType);
                }

                // If persistence type is configured to the instance, use that
                if (eventSourceInstance.Options.EventSourceInstanceDataStoreType != null)
                {
                    _logger.LogDebug("Event Source Instance with Id {ID} has manually configured EventSourceInstanceDataStoreType, using that", eventSourceInstance.Id);

                    try
                    {
                        var dataStore = (IEventSourceInstanceDataStore) _serviceProvider.GetRequiredService(eventSourceInstance.Options.EventSourceInstanceDataStoreType);
                        dataStore.EventSourceInstance = eventSourceInstance;
                        dataStore.StateType = eventSourceInstanceStateType;

                        return dataStore;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to create data store for Event Source Instance with Id {ID}. EventSourceInstanceDataStoreType was configured to type {EventSourceInstanceDataStoreType}, make sure the type is available from IServiceProvider and that it implements IEventSourceInstanceDataStore", eventSourceInstance.Id, eventSourceInstance.Options.EventSourceInstanceDataStoreType);

                        throw;
                    }
                }

                if (eventSourceInstance.Options.PersistState)
                {
                    _logger.LogDebug("Persisting state for Event Source Instance with Id {ID}. Creating the data store using system defaults", eventSourceInstance.Id);

                    var optionsValue = _options.Value;

                    var dataStore = optionsValue.CreateDefaultPersistableEventSourceInstanceDataStore(_serviceProvider, eventSourceInstance, eventSourceInstanceStateType);

                    _logger.LogDebug("Created data store of type {DataStoreType} for Event Source Instance with Id {ID} using system defaults", dataStore.GetType().Name, eventSourceInstance.Id);

                    return dataStore;
                }

                _logger.LogDebug("using in memory data store for Event Source Instance with Id {ID}. State will reset after system restart", eventSourceInstance.Id);

                var inMemory = new InMemoryEventSourceInstanceDataStore { EventSourceInstance = eventSourceInstance, StateType = eventSourceInstanceStateType};

                return inMemory;
            });

            return Task.FromResult(result);
        }
    }
}
