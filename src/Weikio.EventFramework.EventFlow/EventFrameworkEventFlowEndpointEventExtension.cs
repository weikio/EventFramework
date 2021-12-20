using System;
using System.Collections.Generic;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventFlow
{
    public class EventFrameworkEventFlowEndpointEventExtension : ICloudEventExtension
    {
        public const string EventFrameworkEventFlowEndpointAttributeName = "eventFramework_eventFlow_endpoint";

        IDictionary<string, object> _attributes = new Dictionary<string, object>();

        public string ChannelValue
        {
            get => _attributes[EventFrameworkEventFlowEndpointAttributeName] as string;
            set => _attributes[EventFrameworkEventFlowEndpointAttributeName] = value;
        }

        public EventFrameworkEventFlowEndpointEventExtension(string channel)
        {
            ChannelValue = channel;
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
            if (string.Equals(key, EventFrameworkEventFlowEndpointAttributeName))
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
            if (string.Equals(name, EventFrameworkEventFlowEndpointAttributeName))
            {
                return typeof(string);
            }

            return null;
        }
    }
}
