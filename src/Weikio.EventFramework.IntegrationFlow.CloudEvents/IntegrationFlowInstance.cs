using System;
using System.Collections.Generic;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class IntegrationFlowInstance 
    {
        private readonly IntegrationFlow _integrationFlow;
        private readonly IntegrationFlowInstanceOptions _options;

        public IntegrationFlowInstanceOptions FlowInstanceOptions
        {
            get => _options;
        }
        
        public IntegrationFlow IntegrationFlow
        {
            get => _integrationFlow;
        }
        
        public string Id
        {
            get => _options.Id;
        }

        public string Description
        {
            get => _integrationFlow.FlowDefinition.Description;
        }

        public Action<EventSourceInstanceOptions> ConfigureEventSourceInstanceOptions
        {
            get => _integrationFlow.ConfigureEventSourceInstanceOptions;
        }

        public List<ChannelComponent<CloudEvent>> Components
        {
            get => _integrationFlow.Components;
        }

        public List<Endpoint<CloudEvent>> Endpoints
        {
            get => _integrationFlow.Endpoints;
        }

        public List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)> Interceptors
        {
            get => _integrationFlow.Interceptors;
        }

        public object Configuration
        {
            get => _options.Configuration;
        }

        public IntegrationFlowDefinition FlowDefinition => _integrationFlow.FlowDefinition;
        public Type EventSourceType => _integrationFlow.EventSourceType;

        public string Source => _integrationFlow.Source;

        public string InputChannel
        {
            get
            {
                return $"system/flows/{Id}";
            }
        }

        public IntegrationFlowInstance(IntegrationFlow integrationFlow, IntegrationFlowInstanceOptions options)
        {
            _integrationFlow = integrationFlow;
            _options = options;
        }
    }
}
