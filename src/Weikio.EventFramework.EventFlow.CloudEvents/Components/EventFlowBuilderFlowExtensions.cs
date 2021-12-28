using System;
using CloudNative.CloudEvents;
using Weikio.EventFramework.EventFlow.Abstractions;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public static class EventFlowBuilderFlowExtensions
    {
        public static EventFlowBuilder Flow(this EventFlowBuilder builder, Action<EventFlowBuilder> buildFlow,
            Predicate<CloudEvent> predicate = null)
        {
            var componentBuilder = new FlowComponentBuilder().Build(buildFlow, predicate);
            builder.Component(componentBuilder);

            return builder;
        }

        public static EventFlowBuilder Flow(this EventFlowBuilder builder, EventFlowDefinition flowDefinition,
            Predicate<CloudEvent> predicate = null, string flowId = null)
        {
            var componentBuilder = new FlowComponentBuilder().Build(flowDefinition, predicate, flowId);
            builder.Component(componentBuilder);

            return builder;
        }
    }
}