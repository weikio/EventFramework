using System.Collections.Generic;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public interface IEventSourceManager
    {
        /// <summary>
        /// Updates the event sources. Not initialized are initialized.
        /// </summary>
        void Update();
        void Add(EventSource item);
        List<EventSource> GetAll();
    }

    public class EventSourceManager : List<EventSource>, IEventSourceManager
    {
        private readonly EventSourceInitializer _initializer;

        public EventSourceManager(IEnumerable<EventSource> eventSources, EventSourceInitializer initializer)
        {
            AddRange(eventSources);
            _initializer = initializer;
        }

        public void Update()
        {
            foreach (var eventSource in this)
            {
                if (eventSource.IsInitialized)
                {
                    continue;
                }
                
                _initializer.Initialize(eventSource);
            }
        }

        public List<EventSource> GetAll()
        {
            return this;
        }
    }
}
