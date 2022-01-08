using System;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Api.SDK;

namespace Weikio.EventFramework.EventSource.Http
{
    public class HttpCloudEventEventSource : ApiEventSourceBase
    {

        protected override Type ApiEventSourceType { get; } = typeof(HttpCloudEventReceiverApi);
        protected override Type ApiEventSourceConfigurationType { get; } = typeof(HttpCloudEventReceiverApiConfiguration);

        public HttpCloudEventEventSource(IServiceProvider serviceProvider, ICloudEventPublisher cloudEventPublisher, HttpCloudEventReceiverApiConfiguration configuration = null) : base(serviceProvider, cloudEventPublisher, configuration)
        {
        }
    }
}
