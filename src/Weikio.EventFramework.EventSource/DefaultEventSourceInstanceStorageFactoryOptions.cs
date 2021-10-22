using System;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class DefaultEventSourceInstanceStorageFactoryOptions
    {
        public Func<IServiceProvider, EventSourceInstance, Type, IEventSourceInstanceDataStore> CreateDefaultPersistableEventSourceInstanceDataStore { get; set; } =
            (provider, instance, eventSourceStateType) =>
            {
                var result = provider.GetRequiredService<IPersistableEventSourceInstanceDataStore>();
                result.EventSourceInstance = instance;
                result.StateType = eventSourceStateType;

                return result;
            };
    }
}
