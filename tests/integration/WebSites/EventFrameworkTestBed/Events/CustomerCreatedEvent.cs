namespace EventFrameworkTestBed.Events
{
    public class CustomerCreatedEvent
    {
        public string Name { get; set; } = "test name";
        public int Age { get; set; } = 30;
    }
    
    public class CustomerDeletedEvent
    {
        public string Name { get; set; }
    }
}
