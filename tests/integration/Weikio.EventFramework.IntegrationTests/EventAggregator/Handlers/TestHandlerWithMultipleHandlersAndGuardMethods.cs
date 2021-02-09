using System.Threading.Tasks;
using EventFrameworkTestBed.Events;

namespace Weikio.EventFramework.IntegrationTests.EventAggregator.Handlers
{
    public class TestHandlerWithMultipleHandlersAndGuardMethods
    {
        public static int HandleCreatedCount { get; set; }
        public static int HandleDeletedCount { get; set; }
        
        public Task<bool> CanHandleCustomerCreated(CustomerCreatedEvent ev)
        {
            return Task.FromResult(ev.Name == "Test Customer");
        }
        
        public Task<bool> CanHandleCustomerDeleted(CustomerDeletedEvent ev)
        {
            return Task.FromResult(ev.Name == "Test Customer");
        }
        
        public Task HandleCustomerCreated(CustomerCreatedEvent ev, string eventType = "CustomerCreatedEvent")
        {
            HandleCreatedCount += 1;
            return Task.CompletedTask;
        }
        
        public Task HandleCustomerDeleted(CustomerDeletedEvent ev, string eventType = "CustomerDeletedEvent")
        {
            HandleDeletedCount += 1;
            return Task.CompletedTask;
        }
    }
}