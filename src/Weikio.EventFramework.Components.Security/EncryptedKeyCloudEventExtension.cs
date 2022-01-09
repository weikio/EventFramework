using System;
using System.Collections.Generic;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Components.Security
{
    public class EncryptedKeyCloudEventExtension : ICloudEventExtension
    {
        public const string EncryptedKeyExtension =  "eventframework_encrypt_key";

        IDictionary<string, object> _attributes = new Dictionary<string, object>();

        public string EncryptedKeyValue
        {
            get => _attributes[EncryptedKeyExtension] as string;
            set => _attributes[EncryptedKeyExtension] = value;
        }

        public EncryptedKeyCloudEventExtension(string encryptedKey)
        {
            EncryptedKeyValue = encryptedKey;
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
            if (string.Equals(key, EncryptedKeyExtension))
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
            if (string.Equals(name, EncryptedKeyExtension))
            {
                return typeof(string);
            }

            return null;
        }
    }
}
