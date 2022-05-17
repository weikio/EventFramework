using System;
using System.Collections.Generic;

namespace Weikio.EventFramework.EventSource.Abstractions
{
    public interface IEventSourceDefinitionProvider
    {
        EventSourceDefinition Get(string name, Version version);
        EventSourceDefinition GetByType(Type type);
        List<EventSourceDefinition> List();
    }
}