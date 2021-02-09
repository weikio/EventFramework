using System.Threading.Tasks;
using EventFrameworkTestBed.Events;

namespace Weikio.EventFramework.IntegrationTests.EventAggregator.Handlers
{
    public class TestHandlerWithTypedGuard
    {
        public static int HandleCount { get; set; }

        public Task<bool> CanHandle(CustomerCreatedEvent ev)
        {
            return Task.FromResult(ev.Name == "Test Customer");
        }
        
        public Task Handle(CustomerCreatedEvent ev)
        {
            HandleCount += 1;
            return Task.CompletedTask;
        }
    }
}