using System;
using Weikio.EventFramework.EventCreator;

[assembly: EventSource("https://assembly.source")]

namespace Weikio.EventFramework.Tests.Shared
{
    public class CustomerCreated
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public CustomerCreated(Guid id, string firstName, string lastName)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
        }
    }

    public class InvoiceCreated
    {
        public int Index { get; set; } = 0;
    }
    
    [EventType("CustomerOrderCreated")]
    [EventSource("https://test.event")]
    public class OrderCreated
    {
    }
    
    public class CustomerDeleted
    {
    }
}
