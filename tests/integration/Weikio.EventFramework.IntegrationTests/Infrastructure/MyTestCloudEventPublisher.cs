using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.IntegrationTests.Infrastructure
{
    public class MyTestCloudEventPublisher : CloudEventPublisher
    {
        public static List<CloudEvent> PublishedEvents = new List<CloudEvent>();

        public override async Task<CloudEvent> Publish(CloudEvent cloudEvent, string gatewayName)
        {
            await base.Publish(cloudEvent, gatewayName);
            PublishedEvents.Add(cloudEvent);

            return cloudEvent;
        }

        public MyTestCloudEventPublisher(ICloudEventGatewayManager gatewayManager, IOptions<CloudEventPublisherOptions> options, ICloudEventCreator cloudEventCreator, 
            IServiceProvider serviceProvider, ICloudEventChannelManager channelManager) : base(gatewayManager, options, cloudEventCreator, serviceProvider, serviceProvider.GetRequiredService<ILogger<CloudEventPublisher>>(), channelManager)
        {
        }
    }
}
