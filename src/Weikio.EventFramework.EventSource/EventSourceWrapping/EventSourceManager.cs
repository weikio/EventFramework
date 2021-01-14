using System.Collections.Generic;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public class EventSourceManager : List<EventSource>
    {
        private readonly EventSourceInitializer _initializer;

        public EventSourceManager(EventSourceInitializer initializer)
        {
            _initializer = initializer;
        }

        public void Update()
        {
            foreach (var eventSource in this)
            {
                _initializer.Initialize(eventSource);
            }
        }
    }
}
