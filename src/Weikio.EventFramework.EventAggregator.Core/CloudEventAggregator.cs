using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.EventAggregator.Core
{
    public class CloudEventAggregator : ICloudEventAggregator
    {
        private readonly ILogger<CloudEventAggregator> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<Handler> _handlers = new List<Handler>();

        public CloudEventAggregator(ILogger<CloudEventAggregator> logger, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _serviceProvider = serviceProvider;
        }

        public virtual void Subscribe(object subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            lock (_handlers)
            {
                if (_handlers.Any(x => x.Matches(subscriber)))
                {
                    return;
                }

                _logger.LogDebug("Subscribed new handler {Handler} to Cloud Event Aggregator", subscriber);

                _handlers.Add(new Handler(subscriber, _loggerFactory));
            }
        }

        public virtual void Unsubscribe(object subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }

            lock (_handlers)
            {
                var handlerFound = _handlers.FirstOrDefault(x => x.Matches(subscriber));

                if (handlerFound == null)
                {
                    return;
                }

                _handlers.Remove(handlerFound);
                
                _logger.LogDebug("Unsubscribed handler {Handler} from Cloud Event Aggregator", subscriber);
            }
        }

        public async Task Publish(CloudEvent cloudEvent)
        {
            if (cloudEvent == null)
            {
                throw new ArgumentNullException(nameof(cloudEvent));
            }

            Handler[] handlersToNotify;

            lock (_handlers)
            {
                handlersToNotify = _handlers.ToArray();
            }

            foreach (var handler in handlersToNotify)
            {
                await handler.Handle(cloudEvent, _serviceProvider);
            }
        }

        private class Handler
        {
            private readonly ILogger _logger;
            private readonly object _handler;
            
            public Handler(object handler, ILoggerFactory loggerFactory)
            {
                _handler = handler;
                _logger = loggerFactory.CreateLogger(typeof(Handler));
            }

            public bool Matches(object instance)
            {
                return _handler == instance;
            }

            public async Task Handle(CloudEvent cloudEvent, IServiceProvider serviceProvider)
            {
                var eventLink = _handler as EventLink;

                if (eventLink == null)
                {
                    return;
                }

                var canHandle = await eventLink.CanHandle(cloudEvent);

                if (!canHandle)
                {
                    return;
                }

                _logger.LogDebug("Handler {Handler} can handle cloud event, handling", this);
                await eventLink.Action(cloudEvent, serviceProvider);
            }
        }
    }
}
