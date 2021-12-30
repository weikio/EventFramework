using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.EventFlow.Abstractions;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public interface IEventFlowBuilder
    {
        string Name { get; set; }
        string Description { get; set; }
        EventFlowDefinition Definition { get; set; }

        Task<EventFlow> Build(IServiceProvider serviceProvider);
        List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)> Interceptors { get; set; }
        List<Func<ComponentFactoryContext, Task<CloudEventsComponent>>> Components { get; set; }
        Version Version { get; set; }
        string Source { get; set; }
    }
}
