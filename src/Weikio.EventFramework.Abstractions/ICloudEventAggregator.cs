using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Abstractions
{
    public interface ICloudEventAggregator
    {
        void Subscribe(object subscriber);
        Task Publish(CloudEvent cloudEvent);
    }
}
