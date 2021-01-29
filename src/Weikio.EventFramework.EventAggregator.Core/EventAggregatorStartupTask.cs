using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.EventAggregator.Core
{
    public class EventLinkStartupTask : IHostedService 
    {
        private readonly IEnumerable<EventLink> _eventLinks;
        private readonly IEnumerable<EventLinkSource> _eventLinkSources;
        private readonly EventLinkInitializer _handlerInitializer;
        private readonly ILogger<EventLinkStartupTask> _logger;

        public EventLinkStartupTask(IEnumerable<EventLink> links, IEnumerable<EventLinkSource> eventLinkSources, EventLinkInitializer handlerInitializer,  
            ILogger<EventLinkStartupTask> logger)
        {
            _eventLinks = links;
            _eventLinkSources = eventLinkSources;
            _handlerInitializer = handlerInitializer;
            _logger = logger;
        }

        public void Execute(CancellationToken cancellationToken)
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
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Execute(cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
