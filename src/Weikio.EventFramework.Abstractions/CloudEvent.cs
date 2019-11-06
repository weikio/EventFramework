using System;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Abstractions
{
    public class CloudEvent<T> : CloudEvent
    {
        public T Object { get; }

        public CloudEvent(T obj, Uri source, string id = null, DateTime? time = null, params ICloudEventExtension[] extensions) : this(obj.GetType().Name,
            source, id, time, extensions)
        {
            Object = obj;
        }

        public CloudEvent(string type, Uri source, string id = null, DateTime? time = null, params ICloudEventExtension[] extensions) : base(type, source, id,
            time, extensions)
        {
        }

        public CloudEvent(CloudEventsSpecVersion specVersion, string type, Uri source, string id = null, DateTime? time = null,
            params ICloudEventExtension[] extensions) : base(specVersion, type, source, id, time, extensions)
        {
        }

        public CloudEvent(CloudEventsSpecVersion specVersion, string type, Uri source, string subject, string id = null, DateTime? time = null,
            params ICloudEventExtension[] extensions) : base(specVersion, type, source, subject, id, time, extensions)
        {
        }
    }
}