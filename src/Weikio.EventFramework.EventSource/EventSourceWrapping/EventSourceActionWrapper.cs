using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public class EventSourceActionWrapper
    {
        private readonly IActionWrapper _defaultActionWrapper;

        public EventSourceActionWrapper(IActionWrapper defaultActionWrapper)
        {
            _defaultActionWrapper = defaultActionWrapper;
        }

        public (Func<object, bool, Task<EventPollingResult>> Action, bool ContainsState) Wrap(MulticastDelegate action)
        {
            var wrappedAction = _defaultActionWrapper.Wrap(action.Method);
            
            Task<EventPollingResult> WrapperRunner(object state, bool isFirstRun)
            {
                var res = wrappedAction.Action.DynamicInvoke(action, state, isFirstRun);
                var taskResult = (Task<EventPollingResult>) res;
                    
                return taskResult;
            }

            return (WrapperRunner, wrappedAction.ContainsState);
        }
    }
}
