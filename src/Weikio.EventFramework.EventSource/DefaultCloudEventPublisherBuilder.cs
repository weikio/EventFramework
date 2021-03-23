using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource
{
    public class DefaultCloudEventPublisherBuilder : ICloudEventPublisherBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICloudEventGatewayManager _gatewayManager;
        private readonly ICloudEventCreator _cloudEventCreator;
        private readonly IChannelManager _channelManager;

        public DefaultCloudEventPublisherBuilder(IServiceProvider serviceProvider, ICloudEventGatewayManager gatewayManager,
            ICloudEventCreator cloudEventCreator, IChannelManager channelManager)
        {
            _serviceProvider = serviceProvider;
            _gatewayManager = gatewayManager;
            _cloudEventCreator = cloudEventCreator;
            _channelManager = channelManager;
        }

        public CloudEventPublisher Build(IOptions<CloudEventPublisherOptions> options)
        {
            var result = new CloudEventPublisher(_gatewayManager, options, _cloudEventCreator, _serviceProvider, _serviceProvider.GetRequiredService<ILogger<CloudEventPublisher>>(), _channelManager);

            return result;
        }
    }
}
