using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Weikio.ApiFramework.Core.Infrastructure;

namespace Weikio.EventFramework.EventSource.Api.SDK.ApiFrameworkIntegration
{
    public class CustomHttpVerbResolver : IEndpointHttpVerbResolver
    {
        public string GetHttpVerb(ActionModel action)
        {
            return "POST";
        }
    }
}