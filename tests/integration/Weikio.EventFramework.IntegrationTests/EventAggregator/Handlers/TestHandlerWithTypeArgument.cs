using System.Threading.Tasks;
using EventFrameworkTestBed.Events;

namespace Weikio.EventFramework.IntegrationTests.EventAggregator.Handlers
{
    public class TestHandlerWithTypeArgument
    {
        public static int HandledCount { get; set; } = 0;
        public static string CreatedCustomer { get; set; }
        
        public Task Handle(CustomerCreatedEvent customerCreated)
        {
            CreatedCustomer = customerCreated.Name;
            HandledCount += 1;
            
            return Task.CompletedTask;
        }
    }
    
    public class AnotherTestHandlerWithTypeArgument
    {
        public static int HandledCount { get; set; } = 0;
        public static string CreatedCustomer { get; set; }
        
        public Task Handle(CustomerCreatedEvent customerCreated)
        {
            CreatedCustomer = customerCreated.Name;
            HandledCount += 1;
            
            return Task.CompletedTask;
        }
    }
    
    public class MultiTestHandlerWithTypeArgument
    {
        public static int HandledCreateCount { get; set; } = 0;
        public static int HandledDeleteCount { get; set; } = 0;

        public Task Handle(CustomerCreatedEvent customerCreated)
        {
            HandledCreateCount += 1;
            
            return Task.CompletedTask;
        }
        
        public Task HandleDelete(CustomerDeletedEvent customerCreated)
        {
            HandledDeleteCount += 1;
            
            return Task.CompletedTask;
        }
    }
}
