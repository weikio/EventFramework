using System.ComponentModel;
using System.Threading.Tasks;
using Weikio.EventFramework.IntegrationTests.Infrastructure;

namespace Weikio.EventFramework.IntegrationTests.EventSource.Sources
{
    [DisplayName("StatefulEventSource")]
    public class StatefulEventSource
    {
        private int _runCount = 0;
        public Task<NewFileEvent> Run()
        {
            var newFileEvent = new NewFileEvent($"{_runCount}.txt");

            _runCount += 1;
            return Task.FromResult(newFileEvent);
        }
    }
}