using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventSource.Abstractions
{
    public interface IEventSourceProvider
    {
        Task Initialize(CancellationToken cancellationToken);
        List<EventSourceDefinition> List();
        EventSource Get(EventSourceDefinition definition);
    }

    public interface IEventSourceInstanceDataStore
    {
        string EventSourceInstanceId { get; }
        Task<bool> HasRun();
        Task<string> LoadState();
        Task Save(string updatedState);
    }
    
    public interface IPersistableEventSourceInstanceDataStore{}
    
    public interface IEventSourceInstanceStorageFactory
    {
        Task<IEventSourceInstanceDataStore> GetStorage(string eventSourceInstanceId);
    }
}
