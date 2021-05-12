using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Channels;

namespace Weikio.EventFramework.EventPublisher
{
    public class CloudEventPublisher : ICloudEventPublisher
    {
        private readonly ILogger<CloudEventPublisher> _logger;
        private readonly IChannelManager _channelManager;
        private readonly CloudEventPublisherOptions _options;

        public CloudEventPublisher(IOptions<CloudEventPublisherOptions> options, ILogger<CloudEventPublisher> logger, IChannelManager channelManager)
        {
            _logger = logger;
            _channelManager = channelManager;
            _options = options.Value;
        }

        public virtual async Task Publish(object obj, string channelName = null)
        {
            try
            {
                if (obj == null)
                {
                    throw new ArgumentNullException(nameof(obj));
                }

                if (string.IsNullOrWhiteSpace(channelName))
                {
                    channelName = _options.DefaultChannelName;
                }

                if (string.IsNullOrWhiteSpace(channelName))
                {
                    throw new ChannelMissingException();
                }

                var channel = _channelManager.Get(channelName);

                await channel.Send(obj);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to publish to channel");

                throw;
            }
        }
    }
}
