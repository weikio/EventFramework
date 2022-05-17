using System.Threading.Tasks;

namespace Weikio.EventFramework.IntegrationTests.EventAggregator.Handlers
{
    public class TestHandler
    {
        public static int HandleCount { get; set; }
        
        public Task Handle()
        {
            HandleCount += 1;
            return Task.CompletedTask;
        }
    }
}