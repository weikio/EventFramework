using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using CloudNative.CloudEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Weikio.EventFramework.Abstractions
{
    public static class CloudEventJsonExtensions
    {
        public static CloudEvent<T> To<T>(this CloudEvent cloudEvent)
        {
            return CloudEvent<T>.Create(cloudEvent);
        }
        
        public static string ToJson(this CloudEvent cloudEvent)
        {
            var jobject = ToJObject(cloudEvent);

            return jobject.ToString(Formatting.Indented);
        }

        public static string ToJson(this IEnumerable<CloudEvent> cloudEvents)
        {
            var objects = new List<JObject>();

            foreach (var cloudEvent in cloudEvents)
            {
                var jobject = ToJObject(cloudEvent);
                objects.Add(jobject);
            }
            
            var arr = new JArray();

            foreach (var obj in objects)
            {
                arr.Add(obj);
            }

            var result = arr.ToString(Formatting.Indented);

            return result;
        }

        public static CloudEvent ToCloudEvent(this string eventString)
        {
            var formatter = new JsonEventFormatter();
            var jObject = JObject.Parse(eventString);

            var result = formatter.DecodeJObject(jObject);

            return result;
        }

        public static HttpContent ToHttpContent(this CloudEvent cloudEvent)
        {
            var json = cloudEvent.ToJson();

            var result = new StringContent(json, Encoding.UTF8, "application/cloudevents+json");

            return result;
        }

        public static HttpContent ToHttpContent(this IEnumerable<CloudEvent> cloudEvents)
        {
            var json = cloudEvents.ToJson();

            var result = new StringContent(json, Encoding.UTF8, "application/cloudevents+json");

            return result;
        }
        
        public static JObject ToJObject(this CloudEvent cloudEvent)
        {
            var jobject = new JObject();

            foreach (var attribute in cloudEvent.GetAttributes())
            {
                if (attribute.Value == null)
                {
                    continue;
                }

                if (attribute.Value is ContentType type && !string.IsNullOrEmpty(type.MediaType))
                {
                    jobject[attribute.Key] = JToken.FromObject(type.ToString());
                }
                else if (cloudEvent.SpecVersion == CloudEventsSpecVersion.V1_0 &&
                         attribute.Key.Equals(CloudEventAttributes.DataAttributeName(cloudEvent.SpecVersion)))
                {
                    switch (attribute.Value)
                    {
                        case Stream value:
                        {
                            using (var binaryReader = new BinaryReader(value))
                            {
                                jobject["data_base64"] = Convert.ToBase64String(binaryReader.ReadBytes((int) binaryReader.BaseStream.Length));
                            }

                            break;
                        }

                        case IEnumerable<byte> bytes:
                            jobject["data_base64"] = Convert.ToBase64String(bytes.ToArray());

                            break;
                        default:
                            jobject["data"] = JToken.FromObject(attribute.Value);

                            break;
                    }
                }
                else
                {
                    jobject[attribute.Key] = JToken.FromObject(attribute.Value);
                }
            }

            return jobject;
        }
    }
}
