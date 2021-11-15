using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.IntegrationFlow.CloudEvents;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.IntegrationFlow
{
    public class CloudEventsIntegrationManagerTests : IntegrationFlowTestBase
    {
        public CloudEventsIntegrationManagerTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public void CanListIntegrationFlows()
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

                services.AddIntegrationFlow<ConfigurationFlow>(new ConfigurationFlow.Config() { TargetChannelName = "testconfig" });

                services.AddIntegrationFlow<ConfigurationFlow>(new ConfigurationFlow.Config() { TargetChannelName = "anothertestconfig" });
            });

            var manager = provider.GetRequiredService<ICloudEventsIntegrationFlowManager>();
            
            Assert.Equal(2, manager.List().Count);
        }
    }
}
