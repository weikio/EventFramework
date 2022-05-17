using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Api.SDK;

namespace Weikio.EventFramework.EventSource.Api
{
    public interface IApiEventSource
    {
        Task<IActionResult> Handle(ICloudEventPublisher cloudEventPublisher);
    }
    
    public interface IApiEventSource<TConfigurationType> : IApiEventSource where TConfigurationType : IApiEventSourceConfiguration
    {
        TConfigurationType Configuration { get; set; }
    }
}
