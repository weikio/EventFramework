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

    public class CustomerCreatedHandler
    {
        public Task Handle(CloudEvent<CustomerCreated> cloudEvent)
        {
            Debug.WriteLine(cloudEvent.Object?.FirstName);

            return Task.CompletedTask;
        }
    }
}
