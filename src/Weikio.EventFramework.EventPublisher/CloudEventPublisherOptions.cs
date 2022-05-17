using Weikio.EventFramework.Channels;

namespace Weikio.EventFramework.EventPublisher
{
    public class CloudEventPublisherOptions
    {
        public string DefaultChannelName { get; set; } = ChannelName.Default;
    }
}
