using System.Threading.Tasks;
using Weikio.EventFramework.EventAggregator.Core;

namespace Weikio.EventFramework.EventCreator.IntegrationTests.Infrastructure
{
    public class EventAggregatorPublisherForTesting
    {
        private readonly ICloudEventCreator _eventCreator;
        private readonly ICloudEventAggregator _cloudEventAggregator;

        public EventAggregatorPublisherForTesting(ICloudEventCreator eventCreator, ICloudEventAggregator cloudEventAggregator)
        {
            _eventCreator = eventCreator;
            _cloudEventAggregator = cloudEventAggregator;
        }

        public async Task Publish(object obj)
        {
            var cloudEvent = _eventCreator.CreateCloudEvent(obj);

            await _cloudEventAggregator.Publish(cloudEvent);
        }
    }
}