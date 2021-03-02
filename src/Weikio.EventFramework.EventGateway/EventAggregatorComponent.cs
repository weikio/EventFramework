using System.Threading.Tasks.Dataflow;
using CloudNative.CloudEvents;
using Weikio.EventFramework.EventAggregator.Core;

namespace Weikio.EventFramework.EventGateway
{
    public class EventAggregatorComponent
    {
        private readonly ICloudEventAggregator _eventAggregator;

        public EventAggregatorComponent(ICloudEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public ITargetBlock<CloudEvent> Create()
        {
            var result = new ActionBlock<CloudEvent>(async ev =>
            {
                await _eventAggregator.Publish(ev);
            });

            return result;
        }
    }
}