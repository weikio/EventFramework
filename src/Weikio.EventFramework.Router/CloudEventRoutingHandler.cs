using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.Router
{
    public class CloudEventRoutingHandler
    {
        private readonly ICloudEventPublisher _cloudEventPublisher;
        private readonly ICloudEventGatewayManager _cloudEventGatewayManager;
        public string IncomingGatewayName { get; set; }
        public string OutgoingGatewayName { get; set; }
        public IServiceProvider ServiceProvider { get; }
        public Predicate<CloudEvent> Filter { get; set; }
        public Func<CloudEvent, IServiceProvider, Task<CloudEvent>> OnRouting { get; set; }

        public CloudEventRoutingHandler(IServiceProvider serviceProvider, ICloudEventPublisher cloudEventPublisher, ICloudEventGatewayManager cloudEventGatewayManager)
        {
            ServiceProvider = serviceProvider;
            _cloudEventPublisher = cloudEventPublisher;
            _cloudEventGatewayManager = cloudEventGatewayManager;
        }

        public Task<bool> CanHandle(CloudEvent cloudEvent)
        {
            if (!string.Equals(cloudEvent.Gateway(), IncomingGatewayName, StringComparison.InvariantCultureIgnoreCase))
            {
                return Task.FromResult(false);
            }

            if (Filter == null)
            {
                return Task.FromResult(true);
            }

            var result = Filter(cloudEvent);

            return Task.FromResult(result);
        }

        public async Task<bool> Handle(CloudEvent cloudEvent)
        {
            var newContext = cloudEvent;

            if (OnRouting != null)
            {
                newContext = await OnRouting(cloudEvent, ServiceProvider);
            }

            var gateway = _cloudEventGatewayManager.Get(OutgoingGatewayName);

            await _cloudEventPublisher.Publish(newContext, gateway.Name);

            return true;
        }
    }
}
