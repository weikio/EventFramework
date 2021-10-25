using System;
using System.Threading.Tasks;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.IntegrationFlow
{
    public class IntegrationFlowBuilder : IBuilder<IntegrationFlow>
    {
        
        public static IntegrationFlowBuilder From()
        {
            var builder = new IntegrationFlowBuilder();

            return builder;
        }

        public static IntegrationFlowBuilder From<TEventSourceType>(Action<EventSourceInstanceOptions> configureInstance = null)
        {
            var builder = new IntegrationFlowBuilder();

            return builder;
        }

        public Task<IntegrationFlow> Build()
        {
            throw new NotImplementedException();
        }
    }

    public class IntegrationFlow
    {
        
    }
    public interface IBuilder<T> where T : class
    {
        Task<T> Build();
    }
}
