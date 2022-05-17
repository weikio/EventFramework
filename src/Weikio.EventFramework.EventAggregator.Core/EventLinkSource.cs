using System;
using System.Collections.Generic;

namespace Weikio.EventFramework.EventAggregator.Core
{
    public class EventLinkSource
    {
        public EventLinkSource(Func<List<EventLink>> factory)
        {
            Factory = factory;
        }

        public Func<List<EventLink>> Factory { get; set; }
    }
}
