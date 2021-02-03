using System;
using System.Collections.Generic;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.Polling
{
    public class CloudEventPublisherFactoryOptions
    {
        public List<Action<CloudEventPublisherOptions>> ConfigureOptions { get; set; }= new List<Action<CloudEventPublisherOptions>>();
    }
}
