using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public interface IEventSourceInstanceManager
    {
        List<EventSourceInstance> GetAll();
        EventSourceInstance Get(Guid id);
        Task<Guid> Create(EventSourceInstanceOptions options);
        Task Start(Guid eventSourceInstanceId);
        Task StartAll();
        Task Stop(Guid eventSourceId);
        Task StopAll();
        Task Remove(Guid eventSourceId);
        Task RemoveAll();
    }
}
