using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.EventLinks.EventLinkFactories
{
    public interface ITypeToEventLinksConverter
    {
        List<EventLink> Create(IServiceProvider provider, Type handlerType, Func<CloudEvent, Task<bool>> canHandle, MulticastDelegate configure);
    }
}
