﻿using Weikio.EventFramework.EventGateway.Http;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Api.SDK;
using Weikio.EventFramework.EventSource.SDK;

namespace Weikio.EventFramework.EventSource.Http
{
    public class HttpCloudEventReceiverApiConfiguration : ApiEventSourceConfigurationBase
    {
        public string PolicyName { get; set; }
        
        public ICloudEventPublisher CloudEventPublisher { get; set; }
    }
}
