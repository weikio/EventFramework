using System;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class EventFlowInstanceFactory
    {
        private readonly Func<IServiceProvider, Task<EventFlowInstance>> _flowFactory;

        public EventFlowInstanceFactory(Func<IServiceProvider, Task<EventFlowInstance>> flowFactory)
        {
            _flowFactory = flowFactory;
        }

        public async Task<EventFlowInstance> Create(IServiceProvider serviceProvider)
        {
            var result = await _flowFactory.Invoke(serviceProvider);

            return result;
        }
    }
}
