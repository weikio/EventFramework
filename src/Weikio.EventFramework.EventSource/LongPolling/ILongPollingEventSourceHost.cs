using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventSource.LongPolling
{
    public interface ILongPollingEventSourceHost
    {
        void Initialize(Func<CancellationToken, IAsyncEnumerable<object>> eventSource);
        Task StartPolling(CancellationToken stoppingToken);
    }
}