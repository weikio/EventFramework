using System;
using System.Collections.Generic;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.EventFlow.Abstractions;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class EventFlowInstance 
    {
        private readonly EventFlow _eventFlow;
        private readonly EventFlowInstanceOptions _options;

        public EventFlowInstanceOptions FlowInstanceOptions
        {
            get => _options;
        }
        
        public EventFlow EventFlow
        {
            get => _eventFlow;
        }
        
        public string Id
        {
            get => _options.Id;
        }

        public string Description
        {
            get => _eventFlow.FlowDefinition.Description;
        }

        public Action<EventSourceInstanceOptions> ConfigureEventSourceInstanceOptions
        {
            get => _eventFlow.ConfigureEventSourceInstanceOptions;
        }

        public List<ChannelComponent<CloudEvent>> Components
        {
            get => _eventFlow.Components;
        }

        public List<(int ComponentId, string ChannelName)> ComponentChannelNames { get; } = new List<(int, string)>();
        
        public List<Endpoint<CloudEvent>> Endpoints
        {
            get => _eventFlow.Endpoints;
        }

        public List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)> Interceptors
        {
            get => _eventFlow.Interceptors;
        }

        public object Configuration
        {
            get => _options.Configuration;
        }

        public EventFlowDefinition FlowDefinition => _eventFlow.FlowDefinition;
        public Type EventSourceType => _eventFlow.EventSourceType;

        public string Source => _eventFlow.Source;
        
        public string InputChannel => _options.InputChannel;

        public string OutputChannel => _options.OutputChannel;

        public List<Step> Steps = new List<Step>();

        public EventFlowInstance(EventFlow eventFlow, EventFlowInstanceOptions options)
        {
            _eventFlow = eventFlow;
            _options = options;
        }
    }
}
