using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.IntegrationTests.IntegrationFlow
{
    public class TestComponent : CloudEventsComponent
    {
        public TestComponent()
        {
            Func = ModifyEv;
        }

        private static Task<CloudEvent> ModifyEv(CloudEvent ev)
        {
            return Task.FromResult(ev);
        }
    }
}