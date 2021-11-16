using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public interface ICloudEventsIntegrationFlowManager
    {
        Task Execute(IntegrationFlowInstance flowInstance);
        Task<IntegrationFlowInstance> Create(IntegrationFlowDefinition flowDefinition, string id = null, string description = null,
            object configuration = null);

        List<IntegrationFlowInstance> List();
    }
}