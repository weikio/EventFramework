using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventAggregator.Core
{
    public class EventLink
    {
        public EventLink(Func<CloudEvent, Task<bool>> canHandle, Func<CloudEvent, IServiceProvider, Task> action)
        {
            CanHandle = canHandle;
            Action = action;
        }

        public Func<CloudEvent, Task<bool>> CanHandle { get; set; }
        public Func<CloudEvent, IServiceProvider, Task> Action { get; set; } 
    }
}
