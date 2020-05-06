using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Weikio.EventFramework.EventSource
{
    public class EventSourceActionWrapperFactory
    {
        private readonly Type _type;
        private readonly Guid _id;

        public EventSourceActionWrapperFactory(Type type, Guid id)
        {
            _type = type;
            _id = id;
        }

        public List<Func<object, bool, Task<object>>> Create(IServiceProvider serviceProvider)
        {
            Func<object, bool, Task<object>> Create(MethodInfo method)
            {
                var wrapper = serviceProvider.GetRequiredService<EventSourceActionWrapper>();

                Task Func(object state)
                {
                    var instance = serviceProvider.GetRequiredService(_type);

                    var resObject = method.Invoke(instance, new[] { state });
                    
                    dynamic taskResult = (Task) resObject;

                    return taskResult;
                }

                var myFunc = wrapper.Create(Func);

                return myFunc;
            }
            
            var publicMethods = _type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
            var eventReturningStatefulMethods = new List<MethodInfo>();

            foreach (var publicMethod in publicMethods)
            {
                var methodReturnType = publicMethod.ReturnType;

                var isTaskWithValue = methodReturnType.IsGenericType;

                if (isTaskWithValue)
                {
                    var returnValType = methodReturnType.GenericTypeArguments.First();
                    if (typeof(ITuple).IsAssignableFrom(returnValType))
                    {
                        // We have a method which returns events and the current state
                        eventReturningStatefulMethods.Add(publicMethod);
                    }
                }
            }

            var result = new List<Func<object, bool, Task<object>>>();
            foreach (var eventReturningStatefulMethod in eventReturningStatefulMethods)
            {
                var m = Create(eventReturningStatefulMethod);
                result.Add(m);
            }

            return result;
        } 
    }
}
