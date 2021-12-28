using System;
using CloudNative.CloudEvents;
using Weikio.EventFramework.EventFlow.Abstractions;
using Weikio.EventFramework.EventFlow.CloudEvents.Components;

// ReSharper disable once CheckNamespace
namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public static class EventFlowBuilderFlowExtensions
    {
        public static EventFlowBuilder Flow(this EventFlowBuilder builder, Action<EventFlowBuilder> buildFlow,
            Predicate<CloudEvent> predicate = null)
        {
            var flowComponent = new FlowComponentBuilder(buildFlow, predicate);
            builder.Component(flowComponent);

            return builder;
        }

        public static EventFlowBuilder Flow(this EventFlowBuilder builder, EventFlowDefinition flowDefinition,
            Predicate<CloudEvent> predicate = null, string flowId = null)
        {
            var flowComponent = new FlowComponentBuilder(flowDefinition, predicate, flowId);
            builder.Component(flowComponent);

            return builder;
        }
    }
}
