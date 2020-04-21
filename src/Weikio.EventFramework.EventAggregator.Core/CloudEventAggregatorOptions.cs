using System;
using System.Collections.Generic;
using Weikio.EventFramework.EventAggregator.Core.EventLinks.EventLinkFactories;

namespace Weikio.EventFramework.EventAggregator.Core
{
    public class CloudEventAggregatorOptions
    {
        public List<Type> TypeToEventLinksHandlerTypes = new List<Type>()
        {
            typeof(PublicTasksToHandlers),
            typeof(CloudEventsToTypeHandlers),
            typeof(GenericCloudEventsToTypeHandlers)
        };
    }
}
