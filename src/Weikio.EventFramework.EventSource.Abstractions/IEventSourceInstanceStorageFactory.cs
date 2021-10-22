using System;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventSource.Abstractions
{
    public interface IEventSourceInstanceStorageFactory
    {
        Task<IEventSourceInstanceDataStore> GetStorage(EventSourceInstance eventSourceInstance, Type eventSourceInstanceStateType);
    }
}
