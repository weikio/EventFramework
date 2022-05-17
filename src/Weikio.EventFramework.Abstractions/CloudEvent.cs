using System;
using CloudNative.CloudEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Weikio.EventFramework.Abstractions
{
    public class CloudEvent<T> : CloudEvent
    {
        public T Object { get; }

        public CloudEvent(T obj, CloudEvent cloudEvent) : base(cloudEvent.SpecVersion, cloudEvent.Type, cloudEvent.Source)
        {
            Object = obj;
        }
        
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

        public static CloudEvent<T> Create(T obj, CloudEvent cloudEvent)
        {
            var result = new CloudEvent<T>(obj, cloudEvent);
            var attributes = result.GetAttributes();
            attributes.Clear();

            foreach (var attribute in cloudEvent.GetAttributes())
            {
                attributes.Add(attribute);
            }

            return result;
        }
        
        public static CloudEvent<T> Create(CloudEvent cloudEvent)
        {
            T obj;
            
            if (cloudEvent.Data is JToken token)
            {
                obj = token.ToObject<T>();
            }
            else
            {
                obj = (T) cloudEvent.Data;
            }
            
            var result = new CloudEvent<T>(obj, cloudEvent);
            var attributes = result.GetAttributes();
            attributes.Clear();

            foreach (var attribute in cloudEvent.GetAttributes())
            {
                attributes.Add(attribute);
            }

            return result;
        }
    }
}
