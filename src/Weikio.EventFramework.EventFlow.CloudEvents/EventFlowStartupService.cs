using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class EventFlowStartupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventFlowStartupService> _logger;
        private readonly ICloudEventFlowManager _flowManager;
        private readonly List<EventFlowInstanceFactory> _flowFactories;

        public EventFlowStartupService(IServiceProvider serviceProvider, ILogger<EventFlowStartupService> logger, ICloudEventFlowManager flowManager,
            IEnumerable<EventFlowInstanceFactory> flowFactories)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _flowManager = flowManager;
            _flowFactories = flowFactories.ToList();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Executing event flows. Flow count: {FlowFactoryCount}",
                _flowFactories.Count);

            foreach (var flowFactory in _flowFactories)
            {
                try
                {
                    _logger.LogDebug("Creating flow from factory");

                    var flow = await flowFactory.Create(_serviceProvider);

                    _logger.LogDebug("Flow {FlowId} built successfully", flow.Id);

                    _logger.LogDebug("Executing flow {FlowId}", flow.Id);

                    await _flowManager.Execute(flow);

                    _logger.LogDebug("Flow {FlowId} executed successfully", flow.Id);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to create and execute flow from factory");
                }
            }
        }
    }
}
