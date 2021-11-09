using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class CloudEventsIntegrationFlow : IntegrationFlowBase<CloudEvent>
    {
        public Type EventSourceType { get; set; }
        public string Source { get; set; }
    }

    public abstract class CloudEventsIntegrationFlowBase
    {
        public IntegrationFlowBuilder Flow { get; protected set; }
    }

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
