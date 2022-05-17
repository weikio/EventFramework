using Microsoft.Extensions.Options;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource
{
    public interface ICloudEventPublisherBuilder
    {
        CloudEventPublisher Build(IOptions<CloudEventPublisherOptions> options);
    }
}
