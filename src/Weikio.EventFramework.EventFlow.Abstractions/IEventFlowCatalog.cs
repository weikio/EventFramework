using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weikio.EventFramework.EventFlow.Abstractions;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public interface IEventFlowCatalog
    {
        Task Initialize(CancellationToken cancellationToken);
        List<EventFlowDefinition> List();
        Type Get(EventFlowDefinition definition);
    }
}
