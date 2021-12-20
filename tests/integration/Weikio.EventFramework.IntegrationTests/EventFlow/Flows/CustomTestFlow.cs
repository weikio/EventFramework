using Weikio.EventFramework.EventFlow.CloudEvents;
using Weikio.EventFramework.IntegrationTests.EventFlow.ComponentsHandlers;

namespace Weikio.EventFramework.IntegrationTests.EventFlow.Flows
{
    public class CustomTestFlow : CloudEventFlowBase
    {
        public Counter HandlerCounter;

        public CustomTestFlow()
        {
            Flow = EventFlowBuilder.From("local")
                .Handle(ev =>
                {
                    HandlerCounter.Increment();
                });
        }
    }
}