using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.EventAggregator.Handlers
{
    public class TestHandlerWithCloudEventArgument
    {
        public static string HandledEventType { get; set; }
        
        public Task Handle(CloudEvent cloudEvent)
        {
            HandledEventType = cloudEvent.Type;
            return Task.CompletedTask;
        }
    }
}