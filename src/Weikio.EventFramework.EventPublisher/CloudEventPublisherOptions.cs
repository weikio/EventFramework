using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.EventGateway;

namespace Weikio.EventFramework.EventPublisher
{
    public class CloudEventPublisherOptions
    {
        public string DefaultGatewayName { get; set; } = GatewayName.Default;

        public Func<IServiceProvider, CloudEvent, Task<CloudEvent>> OnBeforePublish = (provider, cloudEvent) => Task.FromResult(cloudEvent);
    }
}
