using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Abstractions
{
    public interface ICloudEventPublisher
    {
        Task<CloudEvent> Publish(CloudEvent cloudEvent, string gatewayName = GatewayName.Default);

        Task<List<CloudEvent>> Publish(IList<object> objects, string eventTypeName = "", string id = "", Uri source = null,
            string gatewayName = GatewayName.Default);

        Task<CloudEvent> Publish(object obj, string eventTypeName = "", string id = "", Uri source = null,
            string gatewayName = GatewayName.Default);
    }
}
