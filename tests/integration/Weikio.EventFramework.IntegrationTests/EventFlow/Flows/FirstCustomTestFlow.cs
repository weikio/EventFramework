using Weikio.EventFramework.EventFlow.CloudEvents;

namespace Weikio.EventFramework.IntegrationTests.EventFlow.Flows
{
    public class FirstCustomTestFlow : CloudEventFlowBase
    {
        public FirstCustomTestFlow()
        {
            Flow = EventFlowBuilder.From("local")
                .Channel("flowoutput");
        }
    }
}