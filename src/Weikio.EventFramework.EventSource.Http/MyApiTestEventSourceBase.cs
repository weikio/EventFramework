using System;
using Microsoft.Extensions.Logging;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.EventFramework.EventGateway.Http;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.Http
{
    public class MyApiTestEventSourceBase : ApiEventSourceBase
    {
        public MyApiTestEventSourceBase(IServiceProvider serviceProvider, ILogger<MyApiTestEventSourceBase> logger, IEndpointManager endpointManager, 
            IApiProvider apiProvider, ICloudEventPublisher cloudEventPublisher, 
            HttpEventSourceConfiguration configuration = null) : base(serviceProvider, logger, endpointManager, apiProvider, cloudEventPublisher, configuration)
        {
        }

        protected override Type ApiEventSourceType { get; } = typeof(MyTestApi);
        protected override Type ApiEventSourceConfigurationType { get; } = typeof(MyTestApiConfiguration);
    }
}
