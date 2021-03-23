﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Weikio.EventFramework.EventGateway.Http
{
    public class HttpCloudEventReceiverApi
    {
        private readonly ICloudEventGatewayManager _cloudEventGatewayManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _contextAccessor;

        public HttpCloudEventReceiverApi(ICloudEventGatewayManager cloudEventGatewayManager, IAuthorizationService authorizationService, IHttpContextAccessor contextAccessor)
        {
            _cloudEventGatewayManager = cloudEventGatewayManager;
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
                    var jObject = (JObject) token;
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
            //
            // var gateway = _cloudEventGatewayManager.Get(Configuration.GatewayName);
            // var channel = gateway.IncomingChannel;
            //
            // foreach (var receivedEvent in receivedEvents)
            // {
            //     await channel.Writer.WriteAsync(receivedEvent);
            // }
            
            return new OkResult();
        }
    }
}
