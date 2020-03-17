using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.EventAggregator;

namespace Weikio.EventFramework.Abstractions
{
    public interface ICloudEventHandler
    {
        Task CanHandle(ICloudEventContext cloudEventContext);
        
        Task Handle(ICloudEventContext cloudEventContext);
    }
    
    public interface ICloudEventHandler<TCloudEventDataType>
    {
        
    }

    public class EventLink
    {
        public Predicate<CloudEvent> CanHandle { get; set; }
        public Func<CloudEvent, Task> Action { get; set; } 
    }
}
