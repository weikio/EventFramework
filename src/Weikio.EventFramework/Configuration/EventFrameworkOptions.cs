using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventLinks.EventLinkFactories;

namespace Weikio.EventFramework.Configuration
{
    public class EventFrameworkOptions
    {
        public Uri DefaultSource = new Uri("http://localhost/eventframework");
        public string DefaultGatewayName { get; set; } = GatewayName.Default;

        // public List<Type> TypeToEventLinksFactoryTypes = new List<Type>()
        // {
        //     typeof(PublicTasksToEventLinksFactory),
        //     typeof(GenericCloudEventMethodsToEventLinksFactory),
        //     typeof(CloudEventMethodsToEventLinksFactory)
        // };
        
        public List<Type> TypeToEventLinksHandlerTypes = new List<Type>()
        {
            typeof(PublicTasksToHandlers),
            typeof(CloudEventsToTypeHandlers),
            typeof(GenericCloudEventsToTypeHandlers)
        };
    }


}
