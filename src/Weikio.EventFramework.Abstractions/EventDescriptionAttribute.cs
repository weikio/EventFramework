using System;

namespace Weikio.EventFramework.Abstractions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EventDescriptionAttribute : Attribute
    {
        public string EventType { get; }
        public string Subject { get; }

        public EventDescriptionAttribute(string eventType = null, string subject = null)
        {
            EventType = eventType;
            Subject = subject;
        }
    }
}