using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.EventFramework.EventFlow.Abstractions;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public interface ICloudEventFlowManager
    {
        Task<EventFlowInstance> Execute(EventFlowInstance flowInstance);
        Task<EventFlowInstance> Create(EventFlowDefinition flowDefinition, string id = null, string description = null,
            object configuration = null);

        List<EventFlowInstance> List();
        EventFlowInstance Get(string id);
        Task<EventFlowInstance> Execute(EventFlow eventFlow);
        Task<EventFlowInstance> Execute(EventFlow eventFlow, EventFlowInstanceOptions instanceOptions);
    }
}
