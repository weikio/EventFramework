using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class IntegrationFlowStartupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IntegrationFlowStartupService> _logger;
        private readonly ICloudEventsIntegrationFlowManager _flowManager;
        private readonly List<CloudEventsIntegrationFlowFactory> _flowFactories;

        public IntegrationFlowStartupService(IServiceProvider serviceProvider, ILogger<IntegrationFlowStartupService> logger, ICloudEventsIntegrationFlowManager flowManager,
            IEnumerable<CloudEventsIntegrationFlowFactory> flowFactories)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _flowManager = flowManager;
            _flowFactories = flowFactories.ToList();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Executing integration flows. Flow count: {FlowFactoryCount}",
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
