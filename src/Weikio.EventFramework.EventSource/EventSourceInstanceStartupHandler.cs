using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public class EventSourceInstanceStartupHandler : IHostedService
    {
        private readonly IEventSourceProvider _eventSourceProvider;
        private readonly IEventSourceInstanceManager _eventSourceInstanceManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventSourceInstanceStartupHandler> _logger;

        public EventSourceInstanceStartupHandler(IEventSourceProvider eventSourceProvider, IEventSourceInstanceManager eventSourceInstanceManager, IServiceProvider serviceProvider, 
            ILogger<EventSourceInstanceStartupHandler> logger)
        {
            _eventSourceProvider = eventSourceProvider;
            _eventSourceInstanceManager = eventSourceInstanceManager;
            _serviceProvider = serviceProvider;

            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Starting event sourcing. Event source provider and event source instances are initialized on startup");

                await _eventSourceProvider.Initialize(cancellationToken); 

                _logger.LogDebug("Event source provider initialized");
            
                _logger.LogDebug("Creating all the event source instances configured through the IOptions<EventSourceInstanceOptions>");
                // Get these from service provider instead of injecting them. This is because we want to make sure that event source provider is first initialized as some of the 
                // instances use factories which require that provider is up and running.
                var initialInstances = _serviceProvider.GetServices<EventSourceInstanceOptions>().ToList();
            
                if (initialInstances.Count < 0)
                {
                    _logger.LogDebug("No event source instances created on system startup");

                    return;
                }
            
                _logger.LogTrace("Found {InitialInstanceCount} event source instances to create on system startup", initialInstances.Count);

                foreach (var initialInstance in initialInstances)
                {
                    await _eventSourceInstanceManager.Create(initialInstance);
                }

                _logger.LogDebug("Created {InitialInstanceCount} event source instances on system startup", initialInstances.Count);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to start event sourcing");

                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
