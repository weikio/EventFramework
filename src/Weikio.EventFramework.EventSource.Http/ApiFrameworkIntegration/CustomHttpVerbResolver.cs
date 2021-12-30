using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Weikio.ApiFramework.Core.Infrastructure;

namespace Weikio.EventFramework.EventGateway.Http.ApiFrameworkIntegration
{
    public class CustomHttpVerbResolver : IEndpointHttpVerbResolver
    {
        public string GetHttpVerb(ActionModel action)
        {
            return "POST";
        }
    }
}