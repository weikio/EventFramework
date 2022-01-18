using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Api;
using Weikio.EventFramework.EventSource.Api.SDK;

namespace Weikio.EventFramework.EventSource.Http
{
    public class HttpCloudEventSource : IApiEventSource<HttpCloudEventSourceConfiguration>
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public HttpCloudEventSource(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public HttpCloudEventSourceConfiguration Configuration { get; set; }

        public async Task<IActionResult> Handle(ICloudEventPublisher cloudEventPublisher)
        {
            var httpContext = _contextAccessor.HttpContext;

            var jsonReader = new JsonTextReader(new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 8192, true));

            JToken jToken;

            try
            {
                jToken = await JToken.LoadAsync(jsonReader);
            }
            catch (Exception)
            {
                return new BadRequestResult();
            }

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

            await cloudEventPublisher.Publish(receivedEvents);

            return new OkResult();
        }
    }
}
