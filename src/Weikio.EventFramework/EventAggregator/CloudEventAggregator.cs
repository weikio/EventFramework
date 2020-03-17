using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventAggregator
{
    public class CloudEventAggregator : ICloudEventAggregator
    {
        private readonly List<Handler> _handlers = new List<Handler>();

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

                _handlers.Add(new Handler(subscriber));
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
                var handlersFound = _handlers.FirstOrDefault(x => x.Matches(subscriber));

                if (handlersFound != null)
                {
                    _handlers.Remove(handlersFound);
                }
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

            var tasks = handlersToNotify.Select(h => h.Handle(cloudEvent));

            await Task.WhenAll(tasks);

            var deadHandlers = handlersToNotify.Where(h => h.IsDead).ToList();

            if (deadHandlers.Any())
            {
                lock (_handlers)
                {
                    foreach (var item in deadHandlers)
                    {
                        _handlers.Remove(item);
                    }
                }
            }
        }

        private class Handler
        {
            private readonly WeakReference _reference;

            public Handler(object handler)
            {
                _reference = new WeakReference(handler);
            }

            public bool IsDead => _reference.Target == null;

            public WeakReference Reference => _reference;

            public bool Matches(object instance)
            {
                return _reference.Target == instance;
            }

            public Task Handle(CloudEvent cloudEvent)
            {
                var target = _reference.Target;

                if (target == null)
                {
                    return Task.FromResult(false);
                }

                var eventLink = target as EventLink;

                if (eventLink == null)
                {
                    return Task.FromResult(false);
                }
                
                if (!eventLink.CanHandle(cloudEvent))
                {
                    return Task.FromResult(false);
                }

                return eventLink.Action(cloudEvent);
            }
        }
    }
}
