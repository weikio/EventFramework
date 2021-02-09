using System.ComponentModel;
using System.Threading.Tasks;
using Weikio.EventFramework.IntegrationTests.Infrastructure;

namespace Weikio.EventFramework.IntegrationTests.EventSource.Sources
{
    [DisplayName("MultiEventSource")]
    public class MultiEventSource
    {
        public Task<NewFileEvent> Run()
        {
            return Task.FromResult(new NewFileEvent("file.txt"));
        }
        
        public Task<DeletedFileEvent> Deleted()
        {
            return Task.FromResult(new DeletedFileEvent("file.txt"));
        }
    }
}