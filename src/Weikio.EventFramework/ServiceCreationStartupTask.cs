using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.AspNetCore.StartupTasks;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Router;

namespace Weikio.EventFramework
{
    public class ServiceCreationStartupTask : IStartupTask
    {
        private readonly IEnumerable<ICloudEventGateway> _cloudEventGateways;
        private readonly ICloudEventGatewayManager _cloudEventGatewayManager;
        private readonly ILogger<ServiceCreationStartupTask> _logger;

        public ServiceCreationStartupTask(IEnumerable<ICloudEventGateway> cloudEventGateways, ICloudEventGatewayManager cloudEventGatewayManager, 
            ILogger<ServiceCreationStartupTask> logger)
        {
            _cloudEventGateways = cloudEventGateways;
            _cloudEventGatewayManager = cloudEventGatewayManager;
            _logger = logger;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating cloud event gateways.");

            foreach (var cloudEventGateway in _cloudEventGateways)
            {
                _cloudEventGatewayManager.Add(cloudEventGateway.Name, cloudEventGateway);
            }
            
            var gateways = _cloudEventGatewayManager.Gateways.ToList();
            _logger.LogDebug("There's {GatewayCount} gateways to initialize.", gateways.Count);

            await _cloudEventGatewayManager.Update();
            
            _logger.LogInformation("Initialized {GatewayCount} gateways.", gateways.Count);
        }
    }
    
    public class EventAggregatorStartupTask : IStartupTask
    {
        private readonly IEnumerable<ICloudEventHandler> _eventHandlers;
        private readonly HandlerInitializer _handlerInitializer;
        private readonly ILogger<EventAggregatorStartupTask> _logger;

        public EventAggregatorStartupTask(IEnumerable<ICloudEventHandler> eventHandlers, HandlerInitializer handlerInitializer, 
            ILogger<EventAggregatorStartupTask> logger)
        {
            _eventHandlers = eventHandlers;
            _handlerInitializer = handlerInitializer;
            _logger = logger;
        }

        public Task Execute(CancellationToken cancellationToken)
        {
            foreach (var cloudEventHandler in _eventHandlers)
            {
                _handlerInitializer.Initialize(cloudEventHandler);
            }

            return Task.CompletedTask;
        }
    }
}
