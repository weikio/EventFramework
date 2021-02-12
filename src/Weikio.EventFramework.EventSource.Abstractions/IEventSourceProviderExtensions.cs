using System;

namespace Weikio.EventFramework.EventSource.Abstractions
{
    public static class IEventSourceProviderExtensions
    {
        public static EventSource Get(this IEventSourceProvider sourceProvider, string name, Version version)
        {
            return sourceProvider.Get(new EventSourceDefinition(name, version));

        }

        public static EventSource Get(this IEventSourceProvider sourceProvider, string name)
        {
            return sourceProvider.Get(name, Version.Parse("1.0.0.0"));
        }
    }
}