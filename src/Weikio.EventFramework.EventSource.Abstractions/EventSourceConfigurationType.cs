using System;

namespace Weikio.EventFramework.EventSource.Abstractions
{
    public class EventSourceConfigurationType
    {
        /// <summary>
        /// Gets or sets if the event source requires polling
        /// </summary>
        public bool RequiresPolling { get; set; }

        /// <summary>
        /// Gets or sets the type of configuration used by the event source
        /// </summary>
        public Type ConfigurationType { get; set; }

        public EventSourceConfigurationType(bool requiresPolling, Type configurationType)
        {
            RequiresPolling = requiresPolling;
            ConfigurationType = configurationType;
        }

        public static implicit operator Type(EventSourceConfigurationType configurationType)
        {
            return configurationType.ConfigurationType;
        }
    }
}
