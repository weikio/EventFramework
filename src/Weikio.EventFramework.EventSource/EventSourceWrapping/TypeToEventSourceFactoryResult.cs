using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.EventFramework.EventSource.LongPolling;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public class TypeToEventSourceFactoryResult
    {
        public List<(string Id, (Func<string, Task<EventPollingResult>> Action, bool ContainsState) EventSource)> PollingEventSources { get; set; } =
            new List<(string Id, (Func<string, Task<EventPollingResult>> Action, bool ContainsState) EventSource)>();

        public List<LongPollingEventSourceFactory> LongPollingEventSources { get; set; } = new List<LongPollingEventSourceFactory>();
    }
}
