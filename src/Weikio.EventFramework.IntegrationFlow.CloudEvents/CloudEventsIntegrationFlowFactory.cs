using System;
using System.Threading.Tasks;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class CloudEventsIntegrationFlowFactory
    {
        private readonly Func<IServiceProvider, Task<IntegrationFlowInstance>> _flowFactory;

        public CloudEventsIntegrationFlowFactory(Func<IServiceProvider, Task<IntegrationFlowInstance>> flowFactory)
        {
            _flowFactory = flowFactory;
        }

        public async Task<IntegrationFlowInstance> Create(IServiceProvider serviceProvider)
        {
            var result = await _flowFactory.Invoke(serviceProvider);

            return result;
        }
    }
}