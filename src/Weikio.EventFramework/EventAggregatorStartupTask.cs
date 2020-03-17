using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.AspNetCore.StartupTasks;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Router;

namespace Weikio.EventFramework
{
    public class EventLinkStartupTask : IStartupTask
    {
        private readonly IEnumerable<EventLink> _eventHandlers;
        private readonly EventLinkInitializer _handlerInitializer;
        private readonly ILogger<EventLinkStartupTask> _logger;

        public EventLinkStartupTask(IEnumerable<EventLink> links, EventLinkInitializer handlerInitializer, 
            ILogger<EventLinkStartupTask> logger)
        {
            _eventHandlers = links;
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
