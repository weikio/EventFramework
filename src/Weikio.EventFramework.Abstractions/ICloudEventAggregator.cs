using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Abstractions
{
    public interface ICloudEventAggregator
    {
        Task Publish(CloudEvent cloudEvent);
    }
}
