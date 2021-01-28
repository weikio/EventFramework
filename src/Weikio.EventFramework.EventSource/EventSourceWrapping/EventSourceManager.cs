using System;
using System.Collections.Generic;
using System.Linq;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public class DefaultEventSourceManager : List<EventSourceInstance>, IEventSourceManager
    {
        private readonly IEventSourceInitializer _initializer;
        private readonly EventSourceChangeNotifier _changeNotifier;

        public DefaultEventSourceManager(IEnumerable<EventSourceInstance> eventSources, IEventSourceInitializer initializer, EventSourceChangeNotifier changeNotifier )
        {
            AddRange(eventSources);
            _initializer = initializer;
            _changeNotifier = changeNotifier;
        }

        public void Update()
        {
            var sourceInitialized = false;
            
            foreach (var eventSource in this)
            {
                if (eventSource.Status.Status == EventSourceStatusEnum.Initialized || eventSource.Status.Status == EventSourceStatusEnum.Running)
                {
                    continue;
                }

                var initializationResult = _initializer.Initialize(eventSource);

                if (initializationResult == EventSourceStatusEnum.Initialized)
                {
                    sourceInitialized = true;
                }
            }

            if (sourceInitialized)
            {
                _changeNotifier.Notify();
            }
        }

        public List<EventSourceInstance> GetAll()
        {
            return this;
        }

        public void Stop(Guid eventSourceId)
        {
            var eventSource = this.FirstOrDefault(x => x.Id == eventSourceId);

            if (eventSource == null)
            {
                throw new ArgumentException("Unknown event source " + eventSourceId);
            }

            eventSource.Status.UpdateStatus(EventSourceStatusEnum.Stopping, "Stopping");

            var cancellationTokenSource = eventSource.CancellationToken;
            cancellationTokenSource.Cancel();
        }
    }
}
