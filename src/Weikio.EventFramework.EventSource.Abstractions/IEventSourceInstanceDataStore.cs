using System;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventSource.Abstractions
{
    public interface IEventSourceInstanceDataStore
    {
        EventSourceInstance EventSourceInstance { get; set; }
        Type StateType { get; set; }
        Task<bool> HasRun();
        Task<dynamic> LoadState();
        Task Save(dynamic updatedState);
    }
}
