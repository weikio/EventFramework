using System;

namespace Weikio.EventFramework.EventCreator
{
    [AttributeUsage(AttributeTargets.Class |
                           AttributeTargets.Struct)
    ]
    public class EventTypeAttribute : Attribute
    {
        public string EventTypeName { get; }

        public EventTypeAttribute(string eventTypeName)
        {
            EventTypeName = eventTypeName;
        }
    }
}
