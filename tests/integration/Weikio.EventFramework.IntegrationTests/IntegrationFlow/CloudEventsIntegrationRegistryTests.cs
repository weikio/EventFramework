using System;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.IntegrationFlow.CloudEvents;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.IntegrationFlow
{
    public class CloudEventsIntegrationRegistryTests : IntegrationFlowTestBase
    {
        public CloudEventsIntegrationRegistryTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public void CanRegisterIntegrationFlow()
        {
            var server = Init(services =>
            {
                services.RegisterIntegrationFlow<FirstCustomTestFlow>();
            });

            var provider = server.GetRequiredService<IntegrationFlowProvider>();
            var flow = provider.Get("Weikio.EventFramework.IntegrationTests.IntegrationFlow.FirstCustomTestFlow");

            Assert.NotNull(flow);
        }
        
        [Fact]
        public void BuiltFlowsAreRegistered()
        {
            var server = Init(services =>
            {
                services.AddIntegrationFlow(IntegrationFlowBuilder.From().Channel("test"));
            });

            var provider = server.GetRequiredService<IntegrationFlowProvider>();
            var flows = provider.List();

            Assert.NotEmpty(flows);
        }

        [Fact]
        public void UnknownIntegrationFlowThrows()
        {
            var server = Init(services =>
            {
                services.RegisterIntegrationFlow<FirstCustomTestFlow>();
            });

            var provider = server.GetRequiredService<IntegrationFlowProvider>();
            Assert.Throws<UnknownIntegrationFlowException>(() => provider.Get("NotExists"));
        }

        [Fact]
        public async Task CanStartRegisteredIntegrationFlow()
        {
            var handlerCounter = new Counter();

            var server = Init(services =>
            {
                // Flow which moves the events from local to flowoutput channel
                services.RegisterIntegrationFlow<FirstCustomTestFlow>();
                services.AddChannel("local");

                services.AddChannel("flowoutput", (serviceProvider, options) =>
                {
                    options.Endpoint = ev =>
                    {
                        handlerCounter.Increment();
                    };
                });
            });

            // Get the flow from registry
            var manager = server.GetRequiredService<ICloudEventsIntegrationFlowManager>();

            var flow = await manager.Create(
                "Weikio.EventFramework.IntegrationTests.IntegrationFlow.FirstCustomTestFlow");

            // Start the flow
            await manager.Execute(flow);

            // Send a new event
            var channel = server.GetRequiredService<IChannelManager>().Get("local");
            await channel.Send(new CustomerCreatedEvent());

            // Wait for the flow to deliver it to the output
            await ContinueWhen(() => handlerCounter.Get() > 0, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CanUseConfigurationForRegisteredIntegrationFlow()
        {
            var handlerCounter = new Counter();

            var server = Init(services =>
            {
                // Flow which moves the events from local to flowoutput channel
                services.RegisterIntegrationFlow<ConfigurationFlow>();
                services.AddChannel("local");

                services.AddChannel("configuredChannel", (serviceProvider, options) =>
                {
                    options.Endpoint = ev =>
                    {
                        handlerCounter.Increment();
                    };
                });
            });

            // Get the flow from registry
            var manager = server.GetRequiredService<ICloudEventsIntegrationFlowManager>();

            var flow = await manager.Create("Weikio.EventFramework.IntegrationTests.IntegrationFlow.ConfigurationFlow",
                configuration: new ConfigurationFlow.Config() { TargetChannelName = "configuredChannel" });

            // Start the flow
            await manager.Execute(flow);

            // Send a new event
            var channel = server.GetRequiredService<IChannelManager>().Get("local");
            await channel.Send(new CustomerCreatedEvent());

            // Wait for the flow to deliver it to the output
            await ContinueWhen(() => handlerCounter.Get() > 0, timeout: TimeSpan.FromSeconds(5));
        }
    }
}
