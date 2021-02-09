using System.ComponentModel;
using System.Threading.Tasks;
using Weikio.EventFramework.IntegrationTests.Infrastructure;

namespace Weikio.EventFramework.IntegrationTests.EventSource.Sources
{
    [DisplayName("StatelessTestEventSource")]
    public class StatelessTestEventSource
    {
        public Task<NewFileEvent> Run()
        {
            return Task.FromResult(new NewFileEvent("single.txt"));
        }
    }
}