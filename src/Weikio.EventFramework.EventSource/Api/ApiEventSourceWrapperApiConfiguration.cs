using System;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Api.SDK;

namespace Weikio.EventFramework.EventSource.Api
{
    public class ApiEventSourceWrapperApiConfiguration
    {
        public ICloudEventPublisher CloudEventPublisher { get; set; }
        public IApiEventSourceConfiguration EndpointConfiguration { get; set; }
        public Type ApiType { get; set; }
        public Type ApiConfigurationType { get; set; }
    }
}
