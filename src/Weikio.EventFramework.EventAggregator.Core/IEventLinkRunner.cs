using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.EventAggregator.Core
{
    public interface IEventLinkRunner
    {
        Task Handle(CloudEvent cloudEvent, IServiceProvider serviceProvider);
        Task<bool> CanHandle(CloudEvent cloudEvent);

        void Initialize(object handler, MethodInfo handlerMethod, Func<MethodInfo, CloudEvent, List<object>> getArguments,
            CloudEventCriteria criteria, Func<CloudEvent, Task<bool>> canHandle, MethodInfo guardMethod);
    }
}
