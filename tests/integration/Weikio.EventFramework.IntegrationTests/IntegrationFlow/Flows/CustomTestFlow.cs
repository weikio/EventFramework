using Weikio.EventFramework.IntegrationFlow.CloudEvents;

namespace Weikio.EventFramework.IntegrationTests.IntegrationFlow
{
    public class CustomTestFlow : CloudEventsIntegrationFlowBase
    {
        public Counter HandlerCounter;

        public CustomTestFlow()
        {
            Flow = IntegrationFlowBuilder.From("local")
                .Handle(ev =>
                {
                    HandlerCounter.Increment();
                });
        }
    }
}