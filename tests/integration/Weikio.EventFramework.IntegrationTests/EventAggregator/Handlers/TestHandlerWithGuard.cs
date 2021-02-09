using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.IntegrationTests.EventAggregator.Handlers
{
    public class TestHandlerWithGuard
    {
        public static int HandleCount { get; set; }

        public Task<bool> CanHandle(CloudEvent ev)
        {
            return Task.FromResult(false);
        }
        
        public Task Handle()
        {
            HandleCount += 1;
            return Task.CompletedTask;
        }
    }
}