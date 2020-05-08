using System;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventSource
{
    public class EventPublisherSource
    {
        public EventPublisherSource(Func<Task> action)
        {
            Action = action;
        }

        public Func<Task> Action { get; set; }
    }
}