using System;
using System.Collections.Generic;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class IntegrationFlowInstance 
    {
        private readonly Abstractions.IntegrationFlow _integrationFlow;
        private readonly IntegrationFlowInstanceOptions _options;
        public string Id
        {
            get => _options.Id;
        }

        public string Description
        {
            get => _options.Description;
        }

        public Action<EventSourceInstanceOptions> ConfigureEventSourceInstanceOptions
        {
            get => _options.ConfigureEventSourceInstanceOptions;
        }

        public List<ChannelComponent<CloudEvent>> Components
        {
            get => _options.Components;
        }

        public List<Endpoint<CloudEvent>> Endpoints
        {
            get => _options.Endpoints;
        }

        public List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)> Interceptors
        {
            get => _options.Interceptors;
        }

        public object Configuration
        {
            get => _options.Configuration;
        }

        public IntegrationFlowDefinition FlowDefinition => _options.FlowDefinition;
        public Type EventSourceType => _integrationFlow.EventSourceType;

        public string Source => _integrationFlow.Source;

        public IntegrationFlowInstance(Abstractions.IntegrationFlow integrationFlow, IntegrationFlowInstanceOptions options)
        {
            _integrationFlow = integrationFlow;
            _options = options;
        }
    }
}