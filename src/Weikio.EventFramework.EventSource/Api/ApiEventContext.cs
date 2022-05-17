using System;
using Microsoft.AspNetCore.Http;
using Weikio.EventFramework.EventPublisher;
using Endpoint = Weikio.ApiFramework.Abstractions.Endpoint;

namespace Weikio.EventFramework.EventSource.Api
{
    public class ApiEventContext
    {
        public ICloudEventPublisher EventPublisher { get; set; }
        public HttpContext HttpContext { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public Endpoint Endpoint { get; set; }
    }
}
