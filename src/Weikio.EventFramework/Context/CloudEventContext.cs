using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Context
{
    public class CloudEventContext : ICloudEventContext
    {
        public CloudEventContext(CloudEvent cloudEvent, ICloudEventGateway gateway, IIncomingChannel channel)
        {
            CloudEvent = cloudEvent;
            Gateway = gateway;
            Channel = channel;
        }

        public CloudEvent CloudEvent { get; }
        public ICloudEventGateway Gateway { get; }
        public IIncomingChannel Channel { get; }
    }
}
