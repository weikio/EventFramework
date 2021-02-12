using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource.LongPolling
{
    public interface ILongPollingEventSourceHost
    {
        void Initialize(EventSourceInstance eventSourceInstance, Func<CancellationToken, IAsyncEnumerable<object>> pollers,
            CancellationTokenSource cancellationTokenSource);
        Task StartPolling(CancellationToken stoppingToken);
    }
}
