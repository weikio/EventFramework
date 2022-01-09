using System;
using System.Collections.Generic;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Components.Security
{
    public class EncryptedEventCloudEventExtension : ICloudEventExtension
    {
        public const string EncryptedEventExtension = "eventframework_encrypt_event";

        IDictionary<string, object> _attributes = new Dictionary<string, object>();

        public string EncryptedResultValue
        {
            get => _attributes[EncryptedEventExtension] as string;
            set => _attributes[EncryptedEventExtension] = value;
        }

        public EncryptedEventCloudEventExtension(string encryptedResult)
        {
            EncryptedResultValue = encryptedResult;
        }

        public void Attach(CloudEvent cloudEvent)
        {
            var eventAttributes = cloudEvent.GetAttributes();
            if (_attributes == eventAttributes)
            {
                // already done
                return;
            }

            foreach (var attr in _attributes)
            {
                if (attr.Value != null)
                {
                    eventAttributes[attr.Key] = attr.Value;
                }
            }
            
            _attributes = eventAttributes;
        }

        public bool ValidateAndNormalize(string key, ref dynamic value)
        {
            if (string.Equals(key, EncryptedEventExtension))
            {
                if (value is string)
                {
                    return true;
                }

                throw new InvalidOperationException();
            }
            return false;
        }

        public Type GetAttributeType(string name)
        {
            if (string.Equals(name, EncryptedEventExtension))
            {
                return typeof(string);
            }

            return null;
        }
    }
}
