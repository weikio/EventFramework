using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Abstractions
{
    public interface ICloudEventRoute
    {
        Task<bool> CanHandle(CloudEvent cloudEvent);
        Task<bool> Handle(CloudEvent cloudEvent);
    }

    public interface ICloudEventRoute<T> : ICloudEventRoute
    {
        Task<bool> CanHandle(CloudEvent<T> cloudEvent);
        Task<bool> Handle(CloudEvent<T> cloudEvent);
    }
}
