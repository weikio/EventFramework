using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.IntegrationTests.Infrastructure
{
    public class MyTestCloudEventPublisher : CloudEventPublisher
    {
        public static List<CloudEvent> PublishedEvents = new List<CloudEvent>();

        public override async Task Publish(object obj, string channelName = null)
        {
            await base.Publish(obj, channelName);
        }

        public MyTestCloudEventPublisher(IOptions<CloudEventPublisherOptions> options, 
            IServiceProvider serviceProvider, IChannelManager channelManager) : base(options, serviceProvider.GetRequiredService<ILogger<CloudEventPublisher>>(), channelManager)
        {
        }
    }
}
