using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.EventAggregator.Core
{
    public class EventLinkStartupService : IHostedService 
    {
        private readonly IEnumerable<EventLink> _eventLinks;
        private readonly IEnumerable<EventLinkSource> _eventLinkSources;
        private readonly EventLinkInitializer _handlerInitializer;
        private readonly ILogger<EventLinkStartupService> _logger;

        public EventLinkStartupService(IEnumerable<EventLink> links, IEnumerable<EventLinkSource> eventLinkSources, EventLinkInitializer handlerInitializer,  
            ILogger<EventLinkStartupService> logger)
        {
            _eventLinks = links;
            _eventLinkSources = eventLinkSources;
            _handlerInitializer = handlerInitializer;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var eventLink in _eventLinks)
            {
                _handlerInitializer.Initialize(eventLink);
            }
            
            var eventLinks = new List<EventLink>();

            foreach (var eventLinkSource in _eventLinkSources)
            {
                var eventLinksFromSource = eventLinkSource.Factory();
                eventLinks.AddRange(eventLinksFromSource);
            }

            foreach (var eventLink in eventLinks)
            {
                _handlerInitializer.Initialize(eventLink);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Stopping event link startup service");
            return Task.CompletedTask;
        }
    }
}
