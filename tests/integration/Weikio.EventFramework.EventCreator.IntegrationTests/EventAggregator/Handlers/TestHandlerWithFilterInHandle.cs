using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.EventAggregator.Handlers
{
    public class TestHandlerWithFilterInHandle
    {
        public static int HandleCount { get; set; }
        
        public Task Handle(CloudEvent cloudEvent, string eventType = "CustomerCreatedEvent")
        {
            HandleCount += 1;
            return Task.CompletedTask;
        }
    }
}