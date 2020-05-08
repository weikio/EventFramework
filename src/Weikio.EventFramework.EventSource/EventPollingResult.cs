using System.Collections.Generic;

namespace Weikio.EventFramework.EventSource
{
    public class EventPollingResult
    {
        public List<object> NewEvents { get; set; }
        public object NewState { get; set; }
    }
}