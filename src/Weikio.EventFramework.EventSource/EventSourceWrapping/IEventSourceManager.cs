using System;
using System.Collections.Generic;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public interface IEventSourceManager
    {
        /// <summary>
        /// Updates the event sources. Not initialized are initialized.
        /// </summary>
        void Update();

        /// <summary>
        /// Adds a new event source
        /// </summary>
        /// <param name="item"></param>
        void Add(EventSourceInstance item);

        /// <summary>
        /// Returns all the event sources
        /// </summary>
        /// <returns></returns>
        List<EventSourceInstance> GetAll();

        /// <summary>
        /// Stops the event source
        /// </summary>
        /// <param name="eventSourceId"></param>
        void Stop(Guid eventSourceId);
    }
}
