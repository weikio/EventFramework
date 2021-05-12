using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.IntegrationTests.Infrastructure
{
    public class TestCloudEventPublisher : ICloudEventPublisher
    {
        public List<object> PublishedEvents = new List<object>();

        public Task Publish(object obj, string channelName = null)
        {
            PublishedEvents.Add(obj);

            return Task.CompletedTask;
        }
    }
}
