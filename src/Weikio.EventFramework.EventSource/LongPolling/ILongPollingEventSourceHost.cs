using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventSource.LongPolling
{
    public interface ILongPollingEventSourceHost
    {
        void Initialize(EventSourceWrapping.EventSource eventSource, Func<CancellationToken, IAsyncEnumerable<object>> pollers);
        Task StartPolling(CancellationToken stoppingToken);
    }
}
