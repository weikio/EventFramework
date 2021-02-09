using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventSource.Abstractions
{
    public interface IEventSourceProvider
    {
        Task Initialize(CancellationToken cancellationToken);
        List<EventSourceDefinition> List();
        EventSource Get(EventSourceDefinition definition);
        EventSource Get(string name, Version version);
        EventSource Get(string name);
    }

    public static class IEventSourceProviderExtensions
    {
        public static EventSourceDefinition GetByType(this IEventSourceProvider sourceProvider, Type type)
        {
            var all = sourceProvider.List();

            foreach (var def in all)
            {
                var source = sourceProvider.Get(def);

                if (source.EventSourceType == type)
                {
                    return def;
                }
            }

            return null;
        }
    }
}
