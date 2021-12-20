using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventFlow.CloudEvents;
using Weikio.EventFramework.IntegrationTests.EventFlow.ComponentsHandlers;
using Weikio.EventFramework.IntegrationTests.EventFlow.Flows;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.EventFlow
{
    public class CloudEventFlowManagerTests : IntegrationFlowTestBase
    {
        public CloudEventFlowManagerTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public void CanListEventFlows()
        {
            var counter = new Counter();
            var secondCounter = new Counter();

            var provider = Init(services =>
            {
                services.AddChannel("local");

                services.AddChannel("testconfig", (serviceProvider, options) =>
                {
                    options.Endpoint = ev =>
                    {
                        counter.Increment();
                    };
                });

                services.AddChannel("anothertestconfig", (serviceProvider, options) =>
                {
                    options.Endpoint = ev =>
                    {
                        secondCounter.Increment();
                    };
                });

                services.AddEventFlow<ConfigurationFlow>(new ConfigurationFlow.Config() { TargetChannelName = "testconfig" });

                services.AddEventFlow<ConfigurationFlow>(new ConfigurationFlow.Config() { TargetChannelName = "anothertestconfig" });
            });

            var manager = provider.GetRequiredService<ICloudEventFlowManager>();
            
            Assert.Equal(2, manager.List().Count);
        }
    }
}
