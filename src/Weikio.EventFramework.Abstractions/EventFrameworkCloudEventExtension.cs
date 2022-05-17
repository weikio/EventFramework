using System;
using System.Collections.Generic;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.Abstractions
{
    public class EventFrameworkCloudEventExtension : ICloudEventExtension
    {
        public const string EventFrameworkIncomingGatewayAttributeName = "eventFramework_incomingGateway";
        public const string EventFrameworkIncomingChannelAttributeName = "eventFramework_incomingChannel";

        IDictionary<string, object> _attributes = new Dictionary<string, object>();

        public string GatewayValue
        {
            get => _attributes[EventFrameworkIncomingGatewayAttributeName] as string;
            set => _attributes[EventFrameworkIncomingGatewayAttributeName] = value;
        }

        public string ChannelValue
        {
            get => _attributes[EventFrameworkIncomingChannelAttributeName] as string;
            set => _attributes[EventFrameworkIncomingChannelAttributeName] = value;
        }

        public EventFrameworkCloudEventExtension(string gateway = null, string incomingChannel = null)
        {
            GatewayValue = gateway;
            ChannelValue = incomingChannel;
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
            if (string.Equals(key, EventFrameworkIncomingGatewayAttributeName))
            {
                if (value is string)
                {
                    return true;
                }

                throw new InvalidOperationException();
            }

            if (string.Equals(key, EventFrameworkIncomingChannelAttributeName))
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
            if (string.Equals(name, EventFrameworkIncomingGatewayAttributeName))
            {
                return typeof(string);
            }

            if (string.Equals(name, EventFrameworkIncomingChannelAttributeName))
            {
                return typeof(string);
            }

            return null;
        }
    }
}