namespace Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure
{
    public class CounterUpdatedEvent
    {
        public int Count { get; }

        public CounterUpdatedEvent(int count)
        {
            Count = count;
        }
    }
}