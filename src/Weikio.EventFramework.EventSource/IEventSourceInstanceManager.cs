using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public interface IEventSourceInstanceManager
    {
        IEnumerable<EventSourceInstance> GetAll();
        EventSourceInstance Get(string eventSourceInstanceId);
        Task<string> Create(EventSourceInstanceOptions options);
        Task Start(string eventSourceInstanceId);
        Task StartAll();
        Task Stop(string eventSourceInstanceId);
        Task StopAll();
        Task Remove(string eventSourceInstanceId);
        Task RemoveAll();
    }
}
