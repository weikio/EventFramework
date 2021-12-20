using System.Threading.Tasks;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public interface IEventFlowInstanceFactory
    {
        Task<EventFlowInstance> Create(EventFlow eventFlow, EventFlowInstanceOptions options);
    }
}
