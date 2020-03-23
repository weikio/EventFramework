using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.EventAggregator;

namespace Weikio.EventFramework.EventLinks
{
    public interface IEventLinkRunner
    {
        Task Handle(CloudEvent cloudEvent);
        Task<bool> CanHandle(CloudEvent cloudEvent);

        void Initialize(object handler, MethodInfo handlerMethod, Func<MethodInfo, CloudEvent, List<object>> getArguments,
            CloudEventCriteria criteria, Func<CloudEvent, Task<bool>> canHandle, MethodInfo guardMethod);
    }
}
