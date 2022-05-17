using System.Threading.Tasks;
using EventFrameworkTestBed.Events;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.EventAggregator.Handlers
{
    public class TestHandlerWithGenericCloudEventArgument
    {
        public static string CreatedCustomer { get; set; }
        
        public Task Handle(CloudEvent<CustomerCreatedEvent> customerCreated)
        {
            CreatedCustomer = customerCreated.Object.Name;
            return Task.CompletedTask;
        }
    }
}