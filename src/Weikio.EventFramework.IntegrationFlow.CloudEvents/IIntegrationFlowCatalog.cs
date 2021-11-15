using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public interface IIntegrationFlowCatalog
    {
        Task Initialize(CancellationToken cancellationToken);
        List<IntegrationFlowDefinition> List();
        Type Get(IntegrationFlowDefinition definition);
    }
}