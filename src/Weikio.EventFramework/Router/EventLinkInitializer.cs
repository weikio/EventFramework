using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Router
{
    public class EventLinkInitializer
    {
        private readonly ICloudEventAggregator _cloudEventAggregator;

        public EventLinkInitializer(ICloudEventAggregator cloudEventAggregator)
        {
            _cloudEventAggregator = cloudEventAggregator;
        }

        public void Initialize(EventLink handler)
        {
            _cloudEventAggregator.Subscribe(handler);
        }
    }
}
