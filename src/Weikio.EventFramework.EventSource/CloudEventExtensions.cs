using System;
using CloudNative.CloudEvents;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.EventSource
{
    public static class CloudEventExtensions 
    {
        public static Guid? EventSourceId(this CloudEvent cloudEvent)
        {
            if (cloudEvent?.GetAttributes()?.ContainsKey(EventFrameworkEventSourceExtension.EventFrameworkEventSourceAttributeName) == true)
            {
                return (Guid) cloudEvent.GetAttributes()[EventFrameworkEventSourceExtension.EventFrameworkEventSourceAttributeName];
            }

            return null;
        }
    }
}