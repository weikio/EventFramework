using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventAggregator.Core
{
    public interface ICloudEventAggregator
    {
        void Subscribe(object subscriber);
        Task Publish(CloudEvent cloudEvent);
    }
}
