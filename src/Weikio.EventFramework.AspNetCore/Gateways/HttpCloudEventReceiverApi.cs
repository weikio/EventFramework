using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Weikio.EventFramework.Abstractions;

namespace Weikio.EventFramework.AspNetCore.Gateways
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

        public async Task<IActionResult> ReceiveEvent(CloudEvent cloudEvent)
        {
            if (Configuration == null)
            {
                return new StatusCodeResult(500);
            }
            
            // Assert policy
            if (!string.IsNullOrWhiteSpace(Configuration?.PolicyName))
            {
                var httpContext = _contextAccessor.HttpContext;
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