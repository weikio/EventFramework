using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventAggregator
{
    public class HandlerOptions
    {
        public Func<object> HandlerFactory { get; set; }
        public string Criteria { get; set; }
        public object Handler { get; set; }
    }
    
    
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
            private readonly Dictionary<Type, MethodInfo> _supportedHandlers = new Dictionary<Type, MethodInfo>();
            private readonly List<Tuple<CloudEventCriteria, MethodInfo>> _supportedCloudEventTypes = new List<Tuple<CloudEventCriteria, MethodInfo>>();

            public Handler(object handler)
            {
                _reference = new WeakReference(handler);

                // Support attributes
                // Support giving the criteria in constructor
                // Support actions
                // Support default arguments

                // public methods with the cloudEvent as argument
                var methods = handler.GetType().GetTypeInfo().DeclaredMethods.ToList();
                var methodsWithCloudEvent = methods.Where(x => x.GetParameters().Any(p => p.ParameterType == typeof(CloudEvent))).ToList();

                foreach (var methodWithCloudEvent in methodsWithCloudEvent)
                {
                    var supportedEventType = string.Empty;
                    var supportedSource = string.Empty;
                    var supportedSubject = string.Empty;

                    var cloudEventTypeParameter = methodWithCloudEvent.GetParameters()
                        .FirstOrDefault(x =>
                            string.Equals(x.Name, "eventType", StringComparison.InvariantCultureIgnoreCase) && x.ParameterType == typeof(string));

                    if (cloudEventTypeParameter != null)
                    {
                        supportedEventType = cloudEventTypeParameter.DefaultValue as string;
                    }

                    var cloudEventSourceParameter = methodWithCloudEvent.GetParameters()
                        .FirstOrDefault(x => string.Equals(x.Name, "source", StringComparison.InvariantCultureIgnoreCase) && x.ParameterType == typeof(string));

                    if (cloudEventSourceParameter != null)
                    {
                        supportedSource = cloudEventSourceParameter.DefaultValue as string;
                    }

                    var cloudEventSubjectParameter = methodWithCloudEvent.GetParameters()
                        .FirstOrDefault(x =>
                            string.Equals(x.Name, "subject", StringComparison.InvariantCultureIgnoreCase) && x.ParameterType == typeof(string));

                    if (cloudEventSubjectParameter != null)
                    {
                        supportedSubject = cloudEventSubjectParameter.DefaultValue as string;
                    }

                    var criteria = new CloudEventCriteria() { Type = supportedEventType, Source = supportedSource, Subject = supportedSubject };

                    _supportedCloudEventTypes.Add(new Tuple<CloudEventCriteria, MethodInfo>(criteria, methodWithCloudEvent));
                }

                //
                //
                //
                // var interfaces = handler.GetType().GetTypeInfo().ImplementedInterfaces
                //     .Where(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IHandle<>));
                //
                // foreach (var handleInterface in interfaces)
                // {
                //     var type = handleInterface.GetTypeInfo().GenericTypeArguments[0];
                //     var method = handleInterface.GetRuntimeMethod("HandleAsync", new[] { type });
                //
                //     if (method != null)
                //     {
                //         _supportedHandlers[type] = method;
                //     }
                // }
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

                var tasks = _supportedCloudEventTypes
                    .Where(x => x.Item1.CanHandle(cloudEvent));

                var result = new List<Task>();

                foreach (var task in tasks)
                {
                    var arguments = new List<object>();
                    arguments.Add(cloudEvent);

                    foreach (var parameterInfo in task.Item2.GetParameters())
                    {
                        if (!parameterInfo.HasDefaultValue)
                        {
                            continue;
                        }
                        
                        arguments.Add(parameterInfo.DefaultValue);
                    }
                    
                    var res = (Task) task.Item2.Invoke(target, arguments.ToArray());
                    result.Add(res);
                }

                return Task.WhenAll(result);
            }

            public bool Handles(Type messageType)
            {
                return _supportedHandlers.Any(
                    pair => pair.Key.GetTypeInfo().IsAssignableFrom(messageType.GetTypeInfo()));
            }
        }
    }

    public class CloudEventCriteria : IEquatable<CloudEventCriteria>
    {
        public bool Equals(CloudEventCriteria other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Type == other.Type && Source == other.Source && Subject == other.Subject && DataContentType == other.DataContentType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((CloudEventCriteria) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Source, Subject, DataContentType);
        }

        private sealed class CloudEventCriteriaEqualityComparer : IEqualityComparer<CloudEventCriteria>
        {
            public bool Equals(CloudEventCriteria x, CloudEventCriteria y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null))
                {
                    return false;
                }

                if (ReferenceEquals(y, null))
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                return x.Type == y.Type && x.Source == y.Source && x.Subject == y.Subject && x.DataContentType == y.DataContentType;
            }

            public int GetHashCode(CloudEventCriteria obj)
            {
                return HashCode.Combine(obj.Type, obj.Source, obj.Subject, obj.DataContentType);
            }
        }

        public static IEqualityComparer<CloudEventCriteria> CloudEventCriteriaComparer { get; } = new CloudEventCriteriaEqualityComparer();

        public string Type { get; set; }
        public string Source { get; set; }
        public string Subject { get; set; }
        public string DataContentType { get; set; }

        public bool CanHandle(CloudEvent cloudEvent)
        {
            if (!string.IsNullOrWhiteSpace(Type) && !string.Equals(cloudEvent.Type, Type, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Source) && !string.Equals(cloudEvent.Source.ToString(), Source, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Subject) && !string.Equals(cloudEvent.Subject, Subject, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(DataContentType) &&
                !string.Equals(cloudEvent.DataContentType.ToString(), DataContentType, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}
