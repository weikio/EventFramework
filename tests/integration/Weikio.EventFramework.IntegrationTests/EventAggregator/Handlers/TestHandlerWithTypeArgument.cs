using System.Threading.Tasks;
using EventFrameworkTestBed.Events;

namespace Weikio.EventFramework.IntegrationTests.EventAggregator.Handlers
{
    public class TestHandlerWithTypeArgument
    {
        public static string CreatedCustomer { get; set; }
        
        public Task Handle(CustomerCreatedEvent customerCreated)
        {
            CreatedCustomer = customerCreated.Name;
            return Task.CompletedTask;
        }
    }
}