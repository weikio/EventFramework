using System;
using System.Collections.Generic;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventFlow
{
    public class EventFrameworkEventFlowEventExtension : ICloudEventExtension
    {
        public const string EventFrameworkEventFlowAttributeName = "eventFramework_eventFlow";

        IDictionary<string, object> _attributes = new Dictionary<string, object>();

        public string EventFlowValue
        {
            get => _attributes[EventFrameworkEventFlowAttributeName] as string;
            set => _attributes[EventFrameworkEventFlowAttributeName] = value;
        }

        public EventFrameworkEventFlowEventExtension(string eventFlow)
        {
            EventFlowValue = eventFlow;
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
            if (string.Equals(key, EventFrameworkEventFlowAttributeName))
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
            if (string.Equals(name, EventFrameworkEventFlowAttributeName))
            {
                return typeof(string);
            }

            return null;
        }
    }
}
