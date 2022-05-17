using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource
{
    public interface ICloudEventPublisherFactory
    {
        CloudEventPublisher CreatePublisher(string name);
    }
}