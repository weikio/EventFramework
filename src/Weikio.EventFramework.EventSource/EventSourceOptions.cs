using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Weikio.EventFramework.EventSource
{
    public class EventSourceOptions
    {
    }

    /// <summary>
    /// Types can contain multiple event sources. Because of this a event source action factory is created for each event source.
    /// This service is runs at startup and it "unwraps" the registered event source factories.
    /// This must be run before QuartzHostedService. 
    /// </summary>
    public class EventSourceActionWrapperUnwrapperHost : BackgroundService
    {
        private readonly IEnumerable<EventSourceActionWrapperFactory> _factories;
        private readonly IOptionsMonitorCache<JobOptions> _optionsCache;
        private readonly ILogger<EventSourceActionWrapperUnwrapperHost> _logger;
        private readonly IServiceProvider _serviceProvider;

        public EventSourceActionWrapperUnwrapperHost(IEnumerable<EventSourceActionWrapperFactory> factories, IOptionsMonitorCache<JobOptions> optionsCache,
            ILogger<EventSourceActionWrapperUnwrapperHost> logger, IServiceProvider serviceProvider)
        {
            _factories = factories;
            _optionsCache = optionsCache;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factoryList = _factories.ToList();

            _logger.LogDebug("Unwrapping {FactoryCount} event source action factories.", factoryList.Count);

            foreach (var wrapperFactory in factoryList)
            {
                var eventSourceActions = wrapperFactory.Create(_serviceProvider);

                foreach (var eventSourceActionWrapper in eventSourceActions)
                {
                    var opts = new JobOptions { Action = eventSourceActionWrapper };
                    _optionsCache.TryAdd(Guid.Parse("652d9b12-0780-42a1-b3c2-2643bb4f52a8").ToString(), opts);
                }
            }
            
            return Task.CompletedTask;
        }
    }
}
