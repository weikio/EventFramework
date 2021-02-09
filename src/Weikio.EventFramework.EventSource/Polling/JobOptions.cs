using System;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource.Polling
{
    public class JobOptions
    {
        public MulticastDelegate Action { get; set; }
        public bool ContainsState { get; set; }
        public EsInstance EventSource { get; set; }
    }
}
