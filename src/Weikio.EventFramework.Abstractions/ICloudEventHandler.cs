using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.EventAggregator;

namespace Weikio.EventFramework.Abstractions
{
    // public interface ICloudEventHandler
    // {
    //     Task CanHandle(ICloudEventContext cloudEventContext);
    //     
    //     Task Handle(ICloudEventContext cloudEventContext);
    // }
    //
    public interface ICloudEventHandler<TCloudEventDataType>
    {
        
    }

    public class EventLink
    {
        public EventLink(Func<CloudEvent, Task<bool>> canHandle, Func<CloudEvent, Task> action)
        {
            CanHandle = canHandle;
            Action = action;
        }

        public Func<CloudEvent, Task<bool>> CanHandle { get; set; }
        public Func<CloudEvent, Task> Action { get; set; } 
    }

    public class EventLinkSource
    {
        public EventLinkSource(Func<List<EventLink>> factory)
        {
            Factory = factory;
        }

        public Func<List<EventLink>> Factory { get; set; }
        
        // public List<EventLink> Create(IServiceProvider serviceProvider, Type handlerType, Func<CloudEvent, Task<bool>> canHandle,
        //     MulticastDelegate configure = null)
        // {
        //     
        //     return new List<EventLink>();
        // }
    }
}
