using System.Diagnostics;
using System.Threading.Tasks;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Samples.CodeConfiguration
{
    public class CustomerCreated
    {
        public string FirstName { get; set;}
        public string LastName { get; set; }
    }

    public class CustomerDeleted
    {
        public string Id { get; set; }
        public string FirstName { get; set;}
        public string LastName { get; set; }
    }

    public class CustomerCreatedHandler
    {
        public Task Handle(CloudEvent<CustomerCreated> cloudEvent)
        {
            Debug.WriteLine(cloudEvent.Object?.FirstName);

            return Task.CompletedTask;
        }
    }
    
    public class CustomerDeletedHandler
    {
        public Task Handle(CustomerDeleted customerDeleted)
        {
            Debug.WriteLine(customerDeleted.FirstName);

            return Task.CompletedTask;
        }
    }
}
