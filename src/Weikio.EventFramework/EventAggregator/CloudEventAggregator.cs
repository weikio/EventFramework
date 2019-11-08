using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventAggregator
{
    public class CloudEventAggregator : ICloudEventAggregator
    {
        public Task Publish(CloudEvent cloudEvent)
        {
            return Task.CompletedTask;
        }
    }
}
