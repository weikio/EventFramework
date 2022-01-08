using System;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Api.SDK;

namespace Weikio.EventFramework.EventSource.Http
{
    public class HttpCloudEventEventSource : ApiEventSourceBase
    {

        protected override Type ApiEventSourceType { get; } = typeof(HttpCloudEventReceiverApi);
        protected override Type ApiEventSourceConfigurationType { get; } = typeof(HttpCloudEventReceiverApiConfiguration);

        public HttpCloudEventEventSource(IServiceProvider serviceProvider, ICloudEventPublisher cloudEventPublisher, IApiEventSourceConfiguration configuration = null) : base(serviceProvider, cloudEventPublisher, configuration)
        {
        }
    }

    public class HttpCloudEventReceiverApiConfiguration : ApiEventSourceConfigurationBase
    {
    }
}
