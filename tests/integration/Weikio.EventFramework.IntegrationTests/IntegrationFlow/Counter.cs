using System.Threading;

namespace Weikio.EventFramework.IntegrationTests.IntegrationFlow
{
    public class Counter
    {
        private int _count;

        public void Increment()
        {
            Interlocked.Increment(ref _count);
        }

        public int Get()
        {
            return _count;
        }
    }
}