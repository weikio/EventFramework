using System.Threading.Tasks;

namespace Weikio.EventFramework.EventPublisher
{
    public interface ICloudEventPublisher
    {
        Task Publish(object obj, string channelName = null);
    }
}
