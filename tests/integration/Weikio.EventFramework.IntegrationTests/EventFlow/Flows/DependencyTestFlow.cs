using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventFlow.CloudEvents;
using Weikio.EventFramework.IntegrationTests.EventFlow.ComponentsHandlers;

namespace Weikio.EventFramework.IntegrationTests.EventFlow.Flows
{
    public class DependencyTestFlow : CloudEventFlowBase
    {
        private readonly ILogger<DependencyTestFlow> _logger;
        private readonly Counter _handlerCounter;

        public DependencyTestFlow(ILogger<DependencyTestFlow> logger, Counter handlerCounter)
        {
            _logger = logger;
            _handlerCounter = handlerCounter;

            Flow = EventFlowBuilder.From("local")
                .Handle(ev =>
                {
                    _handlerCounter.Increment();
                });
        }
    }
}