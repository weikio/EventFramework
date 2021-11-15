using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public interface ICloudEventsIntegrationFlowManager
    {
        Task Execute(CloudEventsIntegrationFlow flow);
        Task<CloudEventsIntegrationFlow> Create(IntegrationFlowDefinition flowDefinition, string id = null, string description = null,
            object configuration = null);

        List<CloudEventsIntegrationFlow> List();
    }
}