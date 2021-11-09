using System.ComponentModel;
using System.Threading.Tasks;
using Weikio.EventFramework.IntegrationTests.Infrastructure;

namespace Weikio.EventFramework.IntegrationTests.EventSource.Sources
{
    [DisplayName("NumberEventSource")]
    public class NumberEventSource
    {
        private int _counter = 0;
        public NumberEventSource()
        {
        }
        
        public Task<CounterEvent> Run()
        {
            _counter += 1;
            
            return Task.FromResult(new CounterEvent()
            {
                CurrentCount = _counter
            });
        }
    }

    public class CounterEvent
    {
        public int CurrentCount { get; set; }
    }
}
