using System;
using CloudNative.CloudEvents;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.EventSource
{
    public static class CloudEventExtensions 
    {
        public static string EventSourceId(this CloudEvent cloudEvent)
        {
            if (cloudEvent?.GetAttributes()?.ContainsKey(EventFrameworkEventSourceExtension.EventFrameworkEventSourceAttributeName) == true)
            {
                return (string) cloudEvent.GetAttributes()[EventFrameworkEventSourceExtension.EventFrameworkEventSourceAttributeName];
            }

            return null;
        }
    }
}
