using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventAggregator.Core
{
    public interface ITypeToEventLinksConverter
    {
        List<EventLink> Create(IServiceProvider provider, Type handlerType, Func<CloudEvent, Task<bool>> canHandle, MulticastDelegate configure);
    }
}
