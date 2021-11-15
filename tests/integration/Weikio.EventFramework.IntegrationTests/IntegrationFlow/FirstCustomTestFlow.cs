using Weikio.EventFramework.IntegrationFlow.CloudEvents;

namespace Weikio.EventFramework.IntegrationTests.IntegrationFlow
{
    public class FirstCustomTestFlow : CloudEventsIntegrationFlowBase
    {
        public FirstCustomTestFlow()
        {
            Flow = IntegrationFlowBuilder.From("local")
                .Channel("flowoutput");
        }
    }
}