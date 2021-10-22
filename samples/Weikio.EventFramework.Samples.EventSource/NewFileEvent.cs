namespace Weikio.EventFramework.Samples.EventSource
{
    public class CountEvent
    {
        public int Count { get; set; }
        public CountEvent(int newCount)
        {
            Count = newCount;
        }
    }
}
