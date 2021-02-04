using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public interface IEventSourceInstanceManager
    {
        List<EsInstance> GetAll();
        EsInstance Get(Guid id);
        Task<Guid> Create(EventSourceInstanceOptions options);
        Task Start(Guid eventSourceInstanceId);
        Task StartAll();
        Task Stop(Guid eventSourceId);
        Task StopAll();
        Task Remove(Guid eventSourceId);
        Task RemoveAll();
    }
}
