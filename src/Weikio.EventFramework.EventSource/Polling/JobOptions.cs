using System;

namespace Weikio.EventFramework.EventSource.Polling
{
    public class JobOptions
    {
        public MulticastDelegate Action { get; set; }
        public bool ContainsState { get; set; }
    }
}
