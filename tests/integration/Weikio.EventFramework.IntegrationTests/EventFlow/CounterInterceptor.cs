using System.Threading.Tasks;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.EventFlow
{
    public class CounterInterceptor : IChannelInterceptor
    {
        public int Counter { get; private set; }

        public CounterInterceptor()
        {
        }

        public Task<object> Intercept(object obj)
        {
            Counter += 1;

            return Task.FromResult(obj);
        }
    }

}
