using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public class EventSourceActionWrapper
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICloudEventPublisher _publisher;
        private readonly ILogger<EventSourceActionWrapper> _logger;
        private readonly DefaultActionWrapper _defaultActionWrapper;

        public EventSourceActionWrapper(IServiceProvider serviceProvider, ICloudEventPublisher publisher, ILogger<EventSourceActionWrapper> logger, DefaultActionWrapper defaultActionWrapper)
        {
            _serviceProvider = serviceProvider;
            _publisher = publisher;
            _logger = logger;
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
