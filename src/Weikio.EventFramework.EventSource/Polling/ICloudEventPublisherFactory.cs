using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.Polling
{
    public interface ICloudEventPublisherFactory
    {
        CloudEventPublisher CreatePublisher(string name);
    }
}