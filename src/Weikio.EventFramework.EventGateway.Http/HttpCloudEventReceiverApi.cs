using System.IO;
using System.Text;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Weikio.EventFramework.Abstractions;

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
            var jObject = await JObject.LoadAsync(jsonReader);

            var cloudEventFormatter = new JsonEventFormatter();
            var cloudEvent = cloudEventFormatter.DecodeJObject(jObject);
            
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
            
            var gateway = _cloudEventGatewayManager.Get(Configuration.GatewayName);
            var channel = gateway.IncomingChannel;

            await channel.Writer.WriteAsync(cloudEvent);
            
            return new OkResult();
        }
    }
}
