using System.Threading.Tasks;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public interface IIntegrationFlowInstanceFactory
    {
        Task<IntegrationFlowInstance> Create(IntegrationFlow integrationFlow, IntegrationFlowInstanceOptions options);
    }
}