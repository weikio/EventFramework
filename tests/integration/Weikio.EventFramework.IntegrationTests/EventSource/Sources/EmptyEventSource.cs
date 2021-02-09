using System.ComponentModel;
using System.Threading.Tasks;
using Weikio.EventFramework.IntegrationTests.Infrastructure;

namespace Weikio.EventFramework.IntegrationTests.EventSource.Sources
{
    [DisplayName("EmptyEventSource")]
    public class EmptyEventSource
    {
        public Task<NewFileEvent> Run()
        {
            return Task.FromResult<NewFileEvent>(null);
        }
    }
}