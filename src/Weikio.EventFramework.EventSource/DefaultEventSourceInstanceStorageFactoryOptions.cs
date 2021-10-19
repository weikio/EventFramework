using System;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class DefaultEventSourceInstanceStorageFactoryOptions
    {
        public Func<IServiceProvider, string, IEventSourceInstanceDataStore> CreateDefaultPersistableEventSourceInstanceDataStore = (provider, esInstanceId) => new FileEventSourceInstanceDataStore(esInstanceId);
    }
}
