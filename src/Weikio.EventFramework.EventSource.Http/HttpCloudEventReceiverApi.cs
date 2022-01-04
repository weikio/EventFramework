using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Weikio.ApiFramework.Abstractions;
using Weikio.ApiFramework.Core.Endpoints;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventGateway.Http
{
    public class MyApiTestEventSource : ApiEventSource
    {
        public MyApiTestEventSource(ILogger<MyApiTestEventSource> logger, IEndpointManager endpointManager, 
            IApiProvider apiProvider, ICloudEventPublisher cloudEventPublisher, 
            HttpEventSourceConfiguration configuration = null) : base(logger, endpointManager, apiProvider, cloudEventPublisher, configuration)
        {
        }

        protected override Type ApiEventSourceType { get; } = typeof(MyTestApi);
    }
    
    public class MyTestApi
    {
        public PublisherConfig Configuration { get; set; }
        
        public async Task<IActionResult> Handle()
        {
            var ev = new MyTestEvent();
            
            await Configuration.CloudEventPublisher.Publish(ev);
            
            return new OkResult();
        }
    }

    public class MyTestEvent
    {
        public string Name { get; set; }
    }

    public class HttpCloudEventReceiverApi
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _contextAccessor;

        public HttpCloudEventReceiverApi(IAuthorizationService authorizationService,
            IHttpContextAccessor contextAccessor)
        {
            _authorizationService = authorizationService;
            _contextAccessor = contextAccessor;
        }

        public HttpCloudEventReceiverApiConfiguration Configuration { get; set; }

        public async Task<IActionResult> ReceiveEvent()
        {
            var httpContext = _contextAccessor.HttpContext;

            var jsonReader = new JsonTextReader(new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 8192, true));
            var jToken = await JToken.LoadAsync(jsonReader);

            var cloudEventFormatter = new JsonEventFormatter();

            CloudEvent[] receivedEvents;

            if (jToken is JArray jArray)
            {
                var events = new List<CloudEvent>();

                foreach (var token in jArray)
                {
                    var jObject = (JObject)token;
                    var cloudEvent = cloudEventFormatter.DecodeJObject(jObject);

                    events.Add(cloudEvent);
                }

                receivedEvents = events.ToArray();
            }
            else if (jToken is JObject jObject)
            {
                var cloudEvent = cloudEventFormatter.DecodeJObject(jObject);
                receivedEvents = new[] { cloudEvent };
            }
            else
            {
                throw new Exception("Unknown content type");
            }

            if (Configuration == null)
            {
                return new StatusCodeResult(500);
            }

            // Assert policy
            if (!string.IsNullOrWhiteSpace(Configuration?.PolicyName))
            {
                var user = httpContext.User;

                var authResult = await _authorizationService.AuthorizeAsync(user, Configuration.PolicyName);

                if (!authResult.Succeeded)
                {
                    return new UnauthorizedResult();
                }
            }

            await Configuration.CloudEventPublisher.Publish(receivedEvents);

            return new OkResult();
        }
    }
}
