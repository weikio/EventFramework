using System.Net;
using System.Threading.Tasks;

namespace Weikio.EventFramework.EventSource.Api.SDK
{
    public interface IEventSourceApi<TConfigurationType> : IEventSourceApi
    {
        Task<(HttpStatusCode Result, string Message)> Handle(ApiEventContext eventContext, TConfigurationType configuration);
    }
    
    public interface IEventSourceApi
    {
        Task<(HttpStatusCode Result, string Message)> Handle(ApiEventContext eventContext);
    }
}
