using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure
{
    public class TestCloudEventPublisher : ICloudEventPublisher
    {
        public List<object> PublishedEvents = new List<object>();

        public TestCloudEventPublisher()
        {
        }

        public Task<CloudEvent> Publish(CloudEvent cloudEvent, string gatewayName = GatewayName.Default)
        {
            PublishedEvents.Add(cloudEvent);

            return Task.FromResult<CloudEvent>(cloudEvent);
        }

        public Task<List<CloudEvent>> Publish(IList<object> objects, string eventTypeName = "", string id = "", Uri source = null,
            string gatewayName = GatewayName.Default)
        {
            PublishedEvents.AddRange(objects);

            return Task.FromResult(new List<CloudEvent>());
        }

        public Task<CloudEvent> Publish(object obj, string eventTypeName = "", string id = "", Uri source = null, string gatewayName = GatewayName.Default)
        {
            PublishedEvents.Add(obj);

            return Task.FromResult(new CloudEvent("test", new Uri("http://localhost", UriKind.Absolute), Guid.NewGuid().ToString()));
        }
    }
}
