using System;
using System.Collections.Generic;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventSource.Polling
{
    public class EventFrameworkEventSourceExtension : ICloudEventExtension
    {
        public const string EventFrameworkEventSourceAttributeName = "eventFramework_eventsource";

        IDictionary<string, object> _attributes = new Dictionary<string, object>();

        public Guid EventSourceValue
        {
            get => (Guid) _attributes[EventFrameworkEventSourceAttributeName];
            set => _attributes[EventFrameworkEventSourceAttributeName] = value;
        }

        public EventFrameworkEventSourceExtension(Guid eventSourceId)
        {
            EventSourceValue = eventSourceId;
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
            if (string.Equals(key, EventFrameworkEventSourceAttributeName))
            {
                if (value is Guid)
                {
                    return true;
                }

                throw new InvalidOperationException();
            }

            return false;
        }

        public Type GetAttributeType(string name)
        {
            if (string.Equals(name, EventFrameworkEventSourceAttributeName))
            {
                return typeof(Guid);
            }

            return null;
        }
    }
}