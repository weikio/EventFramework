using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventSource.Abstractions
{
    public interface IEventSourceCatalog
    {
        Task Initialize(CancellationToken cancellationToken);
        List<EventSourceDefinition> List();
        EventSource Get(EventSourceDefinition definition);
    }
}
