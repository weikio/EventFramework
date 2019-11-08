using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Abstractions
{
    public interface ICloudEventContext
    {
        CloudEvent CloudEvent { get; }
        ICloudEventGateway Gateway { get; }
        IIncomingChannel Channel { get; }
    }

    public interface ICloudEventContext<T> : ICloudEventContext
    {
        new CloudEvent<T> CloudEvent { get; }
    }
}
