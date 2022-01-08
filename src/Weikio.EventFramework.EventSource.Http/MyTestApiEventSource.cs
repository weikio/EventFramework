using System;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Api.SDK;

namespace Weikio.EventFramework.EventSource.Http
{
    public class MyTestApiEventSource : ApiEventSourceBase
    {

        protected override Type ApiEventSourceType { get; } = typeof(MyTestApi);
        protected override Type ApiEventSourceConfigurationType { get; } = typeof(MyTestApiConfiguration);

        public MyTestApiEventSource(IServiceProvider serviceProvider, ICloudEventPublisher cloudEventPublisher, IApiEventSourceConfiguration configuration = null) : base(serviceProvider, cloudEventPublisher, configuration)
        {
        }
    }
}
