using System;
using System.Threading.Tasks;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public interface IDataflowChannelInterceptor
    {
        // Task OnBeforeReceive(object obj);
        // Task OnBeforeAdapterLayer(object obj);
        // Task OnBeforeComponentLayer(object obj);
        // Task OnBeforeEndpointLayer(object obj);
        Task<object> Intercept(object obj);
    }
    
    public interface IDataflowChannelInterceptorBase
    {
        // Task OnBeforeReceive(object obj);
        // Task OnBeforeAdapterLayer(object obj);
        // Task OnBeforeComponentLayer(object obj);
        // Task OnBeforeEndpointLayer(object obj);
        Task Intercept(object obj);
    }
    
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

    public enum InterceptorTypeEnum
    {
        PreReceive,
        PostReceive,
        PreAdapters,
        PostAdapters,
        PreComponents,
        PostComponent,
        PreEndpoints
    }
}
