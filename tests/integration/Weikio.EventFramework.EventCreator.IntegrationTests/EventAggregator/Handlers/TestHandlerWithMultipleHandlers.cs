using System.Threading.Tasks;
using EventFrameworkTestBed.Events;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.EventAggregator.Handlers
{
    public class TestHandlerWithMultipleHandlers
    {
        public static int HandleCreatedCount { get; set; }
        public static int HandleDeletedCount { get; set; }
        
        public Task Handle(CustomerCreatedEvent ev, string eventType = "CustomerCreatedEvent")
        {
            HandleCreatedCount += 1;
            return Task.CompletedTask;
        }
        
        public Task Handle(CustomerDeletedEvent ev, string eventType = "CustomerDeletedEvent")
        {
            HandleDeletedCount += 1;
            return Task.CompletedTask;
        }
    }
}