using System;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventGateway
{
    public class CloudEventGatewayOptions
    {
        public Action<string, string, DateTimeOffset, CloudEvent, IServiceProvider> OnMessageRead { get; set; } =
            (gatewayName, channelName, dateTime, cloudEvent, serviceProvider) =>
            {
            };
    }
}
