using System;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.Polling
{
    public class CloudEventPublisherFactoryOptions
    {
        public Action<CloudEventPublisherOptions> ConfigureOptions = options =>
        {

        };
    }
}