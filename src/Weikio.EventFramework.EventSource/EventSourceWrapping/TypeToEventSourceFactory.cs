using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.EventSource.LongPolling;
using Weikio.EventFramework.EventSource.Polling;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    /// <summary>
    /// Implementation which handles the conversion from a .NET Type to one or many event sources.
    /// </summary>
    public class TypeToEventSourceFactory
    {
        private readonly Type _type;
        public string Id { get; }
        private readonly ILogger<TypeToEventSourceFactory> _logger;
        private readonly object _instance;
        private readonly MulticastDelegate _configure;

        public TypeToEventSourceFactory(Type type, Guid id, ILogger<TypeToEventSourceFactory> logger, object instance, MulticastDelegate configure)
        {
            _type = type;
            Id = id.ToString();
            _logger = logger;
            _instance = instance;
            _configure = configure;
        }

        public TypeToEventSourceFactoryResult Create(IServiceProvider serviceProvider)
        {
            // TODO: Currently this class handles all the supported conversions:
            // 1. Methods to polling event sources and
            // 2. Methods to long polling event sources
            // It would be better if we could inject different handlers into this class and each of those could handle the different need.
            // This way developers could add their own conversion implementations
            var publicMethods = _type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(x => x.ReturnType.IsGenericType && typeof(Task<>).IsAssignableFrom(x.ReturnType.GetGenericTypeDefinition()))
                .ToList();

            var longPollingMethods = publicMethods.Where(x =>
                x.ReturnType.IsGenericType && typeof(IAsyncEnumerable<>).IsAssignableFrom(x.ReturnType.GetGenericTypeDefinition())).ToList();

            var taskMethods = publicMethods.Except(longPollingMethods);

            var result = new TypeToEventSourceFactoryResult();

            foreach (var methodInfo in taskMethods)
            {
                var wrapper = ConvertMethodToPollingEventSource(methodInfo, serviceProvider);
                var id = GetId(methodInfo);

                result.PollingEventSources.Add((id, wrapper));
            }

            foreach (var methodInfo in longPollingMethods)
            {
                var wrapper = ConvertMethodToLongPollingServiceFactory(methodInfo, serviceProvider);
                result.LongPollingEventSources.Add(wrapper);
            }

            return result;
        }

        private (Func<object, bool, Task<EventPollingResult>> Action, bool ContainsState) ConvertMethodToPollingEventSource(MethodInfo method,
            IServiceProvider serviceProvider)
        {
            var wrapper = serviceProvider.GetRequiredService<IActionWrapper>();
            var wrappedMethodCall = wrapper.Wrap(method);

            Task<EventPollingResult> WrapperRunner(object state, bool isFirstRun)
            {
                var instance = _instance ?? ActivatorUtilities.CreateInstance(serviceProvider, _type);

                if (_configure != null)
                {
                    _configure.DynamicInvoke(instance);
                }
                
                var del = CreateDelegate(method, instance);

                var res = wrappedMethodCall.Action.DynamicInvoke(del, state, isFirstRun);
                var taskResult = (Task<EventPollingResult>) res;

                return taskResult;
            }

            return (WrapperRunner, wrappedMethodCall.ContainsState);
        }

        private LongPollingEventSourceFactory ConvertMethodToLongPollingServiceFactory(MethodInfo method, IServiceProvider serviceProvider)
        {
            Func<CancellationToken, IAsyncEnumerable<object>> Result()
            {
                var instance = _instance ?? ActivatorUtilities.CreateInstance(serviceProvider, _type);
                var del = CreateDelegate(method, instance);

                var res = (Func<CancellationToken, IAsyncEnumerable<object>>) del;

                return res;
            }

            var factory = new LongPollingEventSourceFactory(Result);

            return factory;
        }

        private static Delegate CreateDelegate(MethodInfo methodInfo, object target)
        {
            var key = GetId(methodInfo);

            var funcTypeFromCache = _cache.GetOrAdd(key, s =>
            {
                var types = methodInfo.GetParameters().Select(p => p.ParameterType);
                types = types.Concat(new[] { methodInfo.ReturnType });

                var funcType = Expression.GetFuncType(types.ToArray());

                return funcType;
            });

            return Delegate.CreateDelegate(funcTypeFromCache, target, methodInfo.Name);
        }

        private static string GetId(MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType?.FullName + methodInfo.Name + "_" + Guid.NewGuid();
        }

        private static readonly ConcurrentDictionary<string, Type> _cache = new ConcurrentDictionary<string, Type>();
    }
}
