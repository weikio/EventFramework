using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventAggregator;

namespace Weikio.EventFramework.Router
{
    public class HandlerInitializer
    {
        private readonly ICloudEventHandlerCollection _handlerCollection;
        private readonly ICloudEventAggregator _cloudEventAggregator;
        private readonly List<HandlerOptions> _handlerOptions = new List<HandlerOptions>();

        public HandlerInitializer(ICloudEventHandlerCollection handlerCollection, ICloudEventAggregator cloudEventAggregator, IEnumerable<IOptions<HandlerOptions>> handlerOptions)
        {
            _handlerCollection = handlerCollection;
            _cloudEventAggregator = cloudEventAggregator;

            foreach (var handlerOption in handlerOptions)
            {
                _handlerOptions.Add(handlerOption.Value);
            }
        }

        public void Initialize(ICloudEventHandler handler)
        {
            _handlerCollection.Add(handler);
            _cloudEventAggregator.Subscribe(handler);
        }
    }
}
