using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.EventSource.Api.SDK;

namespace Weikio.EventFramework.EventSource.Api
{
    public class ApiEventSourceWrapperApi
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _contextAccessor;
        public ApiEventSourceWrapperApiConfiguration Configuration { get; set; }

        public ApiEventSourceWrapperApi(IServiceProvider serviceProvider, IAuthorizationService authorizationService, IHttpContextAccessor contextAccessor)
        {
            _serviceProvider = serviceProvider;
            _authorizationService = authorizationService;
            _contextAccessor = contextAccessor;
        }

        public async Task<IActionResult> Handle()
        {
            if (Configuration == null)
            {
                return new StatusCodeResult(503);
            }

            var httpContext = _contextAccessor.HttpContext;

            if (!string.IsNullOrWhiteSpace(Configuration?.EndpointConfiguration?.AuthorizationPolicy))
            {
                var user = httpContext.User;

                var authResult = await _authorizationService.AuthorizeAsync(user, Configuration?.EndpointConfiguration?.AuthorizationPolicy);

                if (!authResult.Succeeded)
                {
                    return new UnauthorizedResult();
                }
            }

            dynamic handler = ActivatorUtilities.CreateInstance(_serviceProvider, Configuration.ApiType);
            dynamic conf = Convert.ChangeType(Configuration.EndpointConfiguration, Configuration.ApiConfigurationType);
            handler.Configuration = conf;

            var result = await handler.Handle(Configuration.CloudEventPublisher);

            return result;
        }
    }
}
