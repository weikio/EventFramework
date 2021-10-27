using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.IntegrationFlow.CloudEvents;
using Weikio.EventFramework.IntegrationTests.EventSource.Sources;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.IntegrationFlow
{
    public class CloudEventsIntegrationFlowTests : IntegrationFlowTestBase
    {
        public CloudEventsIntegrationFlowTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
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
    
    public class ChannelEndpoint : CloudEventsEndpoint
    {
        public ChannelEndpoint()
        {
            
        }
    }
}
