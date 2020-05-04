namespace EventFrameworkTestBed.Events
{
    public class CustomerCreatedEvent
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
    
    public class CustomerDeletedEvent
    {
        public string Name { get; set; }
    }
}
