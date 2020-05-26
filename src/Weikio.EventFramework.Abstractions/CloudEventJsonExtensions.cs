using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using CloudNative.CloudEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Weikio.EventFramework.Abstractions
{
    public static class CloudEventJsonExtensions
    {
        public static string ToJson(this CloudEvent cloudEvent)
        {
            var jobject = ToJObject(cloudEvent);

            return jobject.ToString(Formatting.Indented);
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
