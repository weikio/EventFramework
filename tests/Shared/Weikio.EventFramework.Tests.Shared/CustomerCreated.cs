using System;

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
}
