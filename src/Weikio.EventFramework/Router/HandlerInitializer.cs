using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.Router
{
    public class HandlerInitializer
    {
        private readonly ICloudEventHandlerCollection _handlerCollection;
        private readonly ICloudEventAggregator _cloudEventAggregator;

        public HandlerInitializer(ICloudEventHandlerCollection handlerCollection, ICloudEventAggregator cloudEventAggregator)
        {
            _handlerCollection = handlerCollection;
            _cloudEventAggregator = cloudEventAggregator;
        }

        public void Initialize(ICloudEventHandler handler)
        {
            _handlerCollection.Add(handler);
            _cloudEventAggregator.Subscribe(handler);
        }
    }
}
