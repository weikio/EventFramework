using System;

namespace Weikio.EventFramework.EventSource.Abstractions
{
    public static class IEventSourceCatalogExtensions
    {
        public static EventSource Get(this IEventSourceCatalog catalog, string name, Version version)
        {
            return catalog.Get(new EventSourceDefinition(name, version));
        }

        public static EventSource Get(this IEventSourceCatalog catalog, string name)
        {
            return catalog.Get(name, Version.Parse("1.0.0.0"));
        }
    }
}