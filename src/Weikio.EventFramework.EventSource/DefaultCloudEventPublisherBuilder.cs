using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource
{
    public class DefaultCloudEventPublisherBuilder : ICloudEventPublisherBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IChannelManager _channelManager;

        public DefaultCloudEventPublisherBuilder(IServiceProvider serviceProvider, IChannelManager channelManager)
        {
            _serviceProvider = serviceProvider;
            _channelManager = channelManager;
        }

        public CloudEventPublisher Build(IOptions<CloudEventPublisherOptions> options)
        {
            var result = new CloudEventPublisher(options, _serviceProvider.GetRequiredService<ILogger<CloudEventPublisher>>(), _channelManager);

            return result;
        }
    }
}
