using System;
using System.Collections.Generic;
using System.Threading;

namespace Weikio.EventFramework.EventSource.LongPolling
{
    public class LongPollingEventSourceFactory
    {
        public Func<Func<CancellationToken, IAsyncEnumerable<object>>> Source { get; }

        public LongPollingEventSourceFactory(Func<Func<CancellationToken, IAsyncEnumerable<object>>> eventSource)
        {
            Source = eventSource;
        }
    }
}