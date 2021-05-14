using System;
using System.Threading.Tasks;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public interface IDataflowChannelInterceptor<in TInput, in TOutput> 
    {
        Task OnPreReceive(TInput obj)
        {
            return Task.CompletedTask;
        }

        Task OnPreAdapterLayer(TInput obj)
        {
            return Task.CompletedTask;
        }

        Task OnPostAdapterLayer(TInput obj)
        {
            return Task.CompletedTask;
        }
        
        Task OnPreComponentLayer(TOutput obj)
        {
            return Task.CompletedTask;
        }

        Task OnPostComponentLayer(TOutput obj)
        {
            return Task.CompletedTask;
        }

        Task OnPreEndpointLayer(TOutput obj)
        {
            return Task.CompletedTask;
        }
    }
}
