using System.Threading.Tasks;

namespace Weikio.EventFramework.Channels.Dataflow.Abstractions
{
    public interface IChannelInterceptorBase
    {
        // Task OnBeforeReceive(object obj);
        // Task OnBeforeAdapterLayer(object obj);
        // Task OnBeforeComponentLayer(object obj);
        // Task OnBeforeEndpointLayer(object obj);
        Task Intercept(object obj);
    }
}
