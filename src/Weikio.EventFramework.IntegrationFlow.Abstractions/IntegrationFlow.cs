using System;

namespace Weikio.EventFramework.IntegrationFlow.Abstractions
{
    public class IntegrationFlow
    {
        public IntegrationFlowDefinition FlowDefinition { get; }
        public Type EventSourceType { get; }
        public string Source { get; }

        public IntegrationFlow(IntegrationFlowDefinition flowDefinition, Type eventSourceType, string source)
        {
            FlowDefinition = flowDefinition;
            EventSourceType = eventSourceType;
            Source = source;
        }
    }
}
