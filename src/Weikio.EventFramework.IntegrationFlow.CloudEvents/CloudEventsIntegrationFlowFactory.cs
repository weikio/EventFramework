using System;
using System.Threading.Tasks;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class CloudEventsIntegrationFlowFactory
    {
        private readonly Func<IServiceProvider, Task<CloudEventsIntegrationFlow>> _flowFactory;

        public CloudEventsIntegrationFlowFactory(Func<IServiceProvider, Task<CloudEventsIntegrationFlow>> flowFactory)
        {
            _flowFactory = flowFactory;
        }

        public async Task<CloudEventsIntegrationFlow> Create(IServiceProvider serviceProvider)
        {
            var result = await _flowFactory.Invoke(serviceProvider);

            return result;
        }
    }
}