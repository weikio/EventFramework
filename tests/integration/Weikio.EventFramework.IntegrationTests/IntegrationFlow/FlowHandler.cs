using System.Threading.Tasks;
using CloudNative.CloudEvents;

namespace Weikio.EventFramework.IntegrationTests.IntegrationFlow
{
    public class FlowHandler
    {
        public Counter Counter { get; set; }

        public Task Handle(CloudEvent ev)
        {
            Counter.Increment();

            return Task.CompletedTask;
        }
    }
}