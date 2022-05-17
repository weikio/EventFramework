using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.Polling
{
    public static class CloudEventPublisherFactoryExtensions
    {
        public static CloudEventPublisher CreatePublisher(this ICloudEventPublisherFactory factory)
        {
            return factory.CreatePublisher(Options.DefaultName);
        }
    }
}