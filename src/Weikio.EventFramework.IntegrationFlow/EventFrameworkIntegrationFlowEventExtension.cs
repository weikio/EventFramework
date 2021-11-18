using System;
using System.Collections.Generic;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.IntegrationFlow
{
    public class EventFrameworkIntegrationFlowEventExtension : ICloudEventExtension
    {
        public const string EventFrameworkIntegrationFlowAttributeName = "eventFramework_integrationFlow";

        IDictionary<string, object> _attributes = new Dictionary<string, object>();

        public string IntegrationFlowValue
        {
            get => _attributes[EventFrameworkIntegrationFlowAttributeName] as string;
            set => _attributes[EventFrameworkIntegrationFlowAttributeName] = value;
        }

        public EventFrameworkIntegrationFlowEventExtension(string integrationFlow)
        {
            IntegrationFlowValue = integrationFlow;
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
            if (string.Equals(key, EventFrameworkIntegrationFlowAttributeName))
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
            if (string.Equals(name, EventFrameworkIntegrationFlowAttributeName))
            {
                return typeof(string);
            }

            return null;
        }
    }
    
    public class EventFrameworkIntegrationFlowEndpointEventExtension : ICloudEventExtension
    {
        public const string EventFrameworkIntegrationFlowEndpointAttributeName = "eventFramework_integrationFlow_endpoint";

        IDictionary<string, object> _attributes = new Dictionary<string, object>();

        public string ChannelValue
        {
            get => _attributes[EventFrameworkIntegrationFlowEndpointAttributeName] as string;
            set => _attributes[EventFrameworkIntegrationFlowEndpointAttributeName] = value;
        }

        public EventFrameworkIntegrationFlowEndpointEventExtension(string channel)
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
            if (string.Equals(key, EventFrameworkIntegrationFlowEndpointAttributeName))
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
            if (string.Equals(name, EventFrameworkIntegrationFlowEndpointAttributeName))
            {
                return typeof(string);
            }

            return null;
        }
    }

}
