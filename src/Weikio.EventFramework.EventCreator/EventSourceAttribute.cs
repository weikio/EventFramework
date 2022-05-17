using System;

namespace Weikio.EventFramework.EventCreator
{
    [AttributeUsage(AttributeTargets.Class |
                           AttributeTargets.Struct | AttributeTargets.Assembly)
    ]
    public class EventSourceAttribute : Attribute
    {
        public Uri EventSourceUri { get; }

        public EventSourceAttribute(string eventSourceUri) : this(new Uri(eventSourceUri))
        {
        }

        public EventSourceAttribute(Uri eventSourceUri)
        {
            EventSourceUri = eventSourceUri;
        }
    }
}
