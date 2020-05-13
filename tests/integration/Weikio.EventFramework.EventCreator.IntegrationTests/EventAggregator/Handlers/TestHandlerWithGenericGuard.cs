using System.Threading.Tasks;
using EventFrameworkTestBed.Events;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.EventAggregator.Handlers
{
    public class TestHandlerWithGenericGuard
    {
        public static int HandleCount { get; set; }

        public Task<bool> CanHandle(CloudEvent<CustomerCreatedEvent> ev)
        {
            return Task.FromResult(ev.Object.Name == "Test Customer");
        }
        
        public Task Handle(CloudEvent<CustomerCreatedEvent> ev)
        {
            HandleCount += 1;
            return Task.CompletedTask;
        }
    }
}