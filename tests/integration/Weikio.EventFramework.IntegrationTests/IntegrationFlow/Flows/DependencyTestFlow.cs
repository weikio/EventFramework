using Microsoft.Extensions.Logging;
using Weikio.EventFramework.IntegrationFlow.CloudEvents;

namespace Weikio.EventFramework.IntegrationTests.IntegrationFlow
{
    public class DependencyTestFlow : CloudEventsIntegrationFlowBase
    {
        private readonly ILogger<DependencyTestFlow> _logger;
        private readonly Counter _handlerCounter;

        public DependencyTestFlow(ILogger<DependencyTestFlow> logger, Counter handlerCounter)
        {
            _logger = logger;
            _handlerCounter = handlerCounter;

            Flow = IntegrationFlowBuilder.From("local")
                .Handle(ev =>
                {
                    _handlerCounter.Increment();
                });
        }
    }
}