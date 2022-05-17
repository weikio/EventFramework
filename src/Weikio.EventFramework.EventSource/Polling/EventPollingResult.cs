using System.Collections.Generic;

namespace Weikio.EventFramework.EventSource.Polling
{
    public class EventPollingResult
    {
        public bool IsFirstRun { get; set; }
        public List<object> NewEvents { get; set; }
        public object NewState { get; set; }
    }
}
