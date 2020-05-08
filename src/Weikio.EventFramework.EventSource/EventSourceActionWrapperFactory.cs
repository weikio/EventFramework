using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Weikio.EventFramework.EventSource
{
    public class EventSourceActionWrapperFactory
    {
        private readonly Type _type;
        public string Id { get; }
        private readonly ILogger<EventSourceActionWrapperFactory> _logger;

        public EventSourceActionWrapperFactory(Type type, Guid id, ILogger<EventSourceActionWrapperFactory> logger)
        {
            _type = type;
            Id = id.ToString();
            _logger = logger;
        }

        public List<(string Id, (Func<object, bool, Task<EventPollingResult>> Action, bool ContainsState) EventSource)> Create(IServiceProvider serviceProvider)
        {
            (Func<object, bool, Task<EventPollingResult>> Action, bool ContainsState) Create(MethodInfo method)
            {
                var wrapper = new Wrapper();
                var wrappedMethodCall = wrapper.Wrap(method);
                
                Task<EventPollingResult> WrapperRunner(object state, bool isFirstRun)
                {
                    var instance = serviceProvider.GetRequiredService(_type);
                    var del = CreateDelegate(method, instance);

                    var res = wrappedMethodCall.Action.DynamicInvoke(del, state, isFirstRun);
                    var taskResult = (Task<EventPollingResult>) res;
                    
                    return taskResult;
                }

                return (WrapperRunner, wrappedMethodCall.ContainsState);
            }

            var publicMethods = _type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();

            var result = new List<(string Id, (Func<object, bool, Task<EventPollingResult>> Action, bool ContainsState) EventSource)>();

            foreach (var methodInfo in publicMethods)
            {
                var wrapper = Create(methodInfo);
                var id = GetId(methodInfo);
                
                result.Add((id, wrapper));
            }

            return result;
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
            return methodInfo.DeclaringType?.FullName + methodInfo.Name;
        }

        private static readonly ConcurrentDictionary<string, Type> _cache = new ConcurrentDictionary<string, Type>();
    }
}
