using System.Threading;

namespace Weikio.EventFramework.EventSource.Polling
{
    public class EventSourceChangeToken
    {
        public void Initialize()
        {
            TokenSource = new CancellationTokenSource();
        }

        public CancellationTokenSource TokenSource { get; private set; } = new CancellationTokenSource();
    }
}