using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.EventSource.LongPolling
{
    public class LongPollingService : List<LongPollingEventSourceFactory>
    {
        private readonly ILogger<LongPollingService> _logger;

        public LongPollingService(IEnumerable<LongPollingEventSourceFactory> factories, ILogger<LongPollingService> logger)
        {
            AddRange(factories);
            _logger = logger;
        }
    }
}