namespace Weikio.EventFramework.EventSource
{
    public class CounterEvent
    {
        public CounterEvent(int count)
        {
            Count = count;
        }

        public int Count { get; }
    }
}
