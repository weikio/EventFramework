using System.ComponentModel;
using System.Threading.Tasks;
using Weikio.EventFramework.IntegrationTests.Infrastructure;

namespace Weikio.EventFramework.IntegrationTests.EventSource.Sources
{
    [DisplayName("StatefulEventSourceWithInitialization")]
    public class StatefulEventSourceWithInitialization
    {
        private int _runCount = 0;
        public Task<NewFileEvent> Run(bool isFirstRun)
        {
            if (isFirstRun)
            {
                _runCount += 10;

                return Task.FromResult<NewFileEvent>(null);
            }
            
            var newFileEvent = new NewFileEvent($"{_runCount}.txt");

            _runCount += 1;
            return Task.FromResult(newFileEvent);
        }
    }
}