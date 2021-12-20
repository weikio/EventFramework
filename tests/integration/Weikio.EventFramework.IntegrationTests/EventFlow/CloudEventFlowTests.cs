using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.EventFlow
{
    public class CloudEventFlowTests : IntegrationFlowTestBase
    {
        public CloudEventFlowTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public void CanRunIntegrationFlow()
        {
            // var serviceProvider = Init(services =>
            // {
            //     services.AddEventSource<TestEventSource>();
            // });
            //
            // var integrationFlow = new CloudEventsIntegrationFlow();
            // integrationFlow.Id = "test";
            // integrationFlow.Components = new List<ChannelComponent<CloudEvent>>();
            // integrationFlow.Components.Add(new ChannelComponent<CloudEvent>(new TestComponent()));
            // // integrationFlow.Endpoints.Add(new CloudEventsEndpoint());
            //
            // integrationFlow.EventSourceInstanceOptions = new EventSourceInstanceOptions()
            // {
            //     EventSourceDefinition = "TestEventSource", PollingFrequency = TimeSpan.FromSeconds(1),
            // };
        }
    }
}
