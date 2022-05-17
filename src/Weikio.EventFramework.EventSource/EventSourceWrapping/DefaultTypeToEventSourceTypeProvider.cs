using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventSource.EventSourceWrapping
{
    public class DefaultTypeToEventSourceTypeProvider : ITypeToEventSourceTypeProvider
    {

        public (List<MethodInfo> PollingSources, List<MethodInfo> LongPollingSources) GetSourceTypes(Type type)
        {
            var publicMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();

            var longPollingMethods = publicMethods.Where(x =>
                x.ReturnType.IsGenericType && typeof(IAsyncEnumerable<>).IsAssignableFrom(x.ReturnType.GetGenericTypeDefinition())).ToList();

            var taskMethods = publicMethods.Except(longPollingMethods)
                .Where(x => x.ReturnType.IsGenericType && typeof(Task<>).IsAssignableFrom(x.ReturnType.GetGenericTypeDefinition())).ToList();

            return (taskMethods, longPollingMethods);
        }
    }
}
