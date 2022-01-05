using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Weikio.EventFramework.EventSource.Api.SDK
{
    public interface IApiEventSource<TConfigurationType> where TConfigurationType : IApiEventSourceConfiguration
    {
        TConfigurationType Configuration { get; set; }
        Task<IActionResult> Handle();
    }
}
