using System;
using System.Linq;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventFlow.CloudEvents;
using Weikio.EventFramework.IntegrationTests.EventFlow.ComponentsHandlers;
using Weikio.EventFramework.IntegrationTests.EventFlow.Flows;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.EventFlow
{
    public class CloudEventFlowRegistryTests : IntegrationFlowTestBase
    {
        public CloudEventFlowRegistryTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public void CanRegisterIntegrationFlow()
        {
            var server = Init(services =>
            {
                services.RegisterEventFlow<FirstCustomTestFlow>();
            });

            var provider = server.GetRequiredService<EventFlowProvider>();
            var flow = provider.Get("Weikio.EventFramework.IntegrationTests.IntegrationFlow.FirstCustomTestFlow");

            Assert.NotNull(flow);
        }

        [Fact]
        public void BuiltFlowsAreRegistered()
        {
            var server = Init(services =>
            {
                services.AddEventFlow(EventFlowBuilder.From().Channel("test"));
            });

            var provider = server.GetRequiredService<EventFlowProvider>();
            var flows = provider.List();

            Assert.NotEmpty(flows);
        }
        
        [Fact]
        public void CanBuiltFlowWithName()
        {
            var server = Init(services =>
            {
                services.AddEventFlow(EventFlowBuilder
                    .From()
                    .Channel("test")
                    .WithName("myflow"));
            });

            var provider = server.GetRequiredService<EventFlowProvider>();
            var flow = provider.Get("myflow");
            
            Assert.NotNull(flow);
        }
        
        [Fact]
        public void CanBuiltFlowWithNameAndVersion()
        {
            var server = Init(services =>
            {
                services.AddEventFlow(EventFlowBuilder
                    .From()
                    .Channel("test")
                    .WithName("myflow")
                    .WithVersion("1.2.5"));
            });

            var provider = server.GetRequiredService<EventFlowProvider>();
            var flow = provider.Get(("myflow", "1.2.5"));
            
            Assert.NotNull(flow);
        }
        
        [Fact]
        public async Task CanCreateInstanceOfFlowWithName()
        {
            var executed = false;
            var server = Init(services =>
            {
                services.AddChannel("local");

                services.AddChannel("test", (serviceProvider, options) =>
                {
                    options.Endpoint = ev => executed = true;
                });
                
                services.AddEventFlow(EventFlowBuilder
                    .From("local")
                    .Channel("test")
                    .WithName("myflow"));
            });

            var provider = server.GetRequiredService<EventFlowProvider>();
            var manager = server.GetRequiredService<ICloudEventFlowManager>();
            var flow = provider.Get("myflow");

            var flowInstance = await flow.Create(server);

            await manager.Execute(flowInstance);

            var msg = "hello";
            var inputChannel = server.GetRequiredService<IChannelManager>().Get("local");

            await inputChannel.Send(msg);

            await ContinueWhen(() => executed);
        }

        [Fact]
        public async Task CanCreateAnotherInstanceOfRunningFlow()
        {
            var server = Init(services =>
            {
                services.AddEventFlow(EventFlowBuilder.From().Channel("test"));
            });

            var provider = server.GetRequiredService<EventFlowProvider>();
            var manager = server.GetRequiredService<ICloudEventFlowManager>();

            var flow = provider.List().First();

            var anotherFlow = await manager.Create(flow, "anotherinstance");
            await manager.Execute(anotherFlow);
        }

        [Fact]
        public void UnknownIntegrationFlowThrows()
        {
            var server = Init(services =>
            {
                services.RegisterEventFlow<FirstCustomTestFlow>();
            });

            var provider = server.GetRequiredService<EventFlowProvider>();
            Assert.Throws<UnknownEventFlowException>(() => provider.Get("NotExists"));
        }

        [Fact]
        public async Task CanStartRegisteredIntegrationFlow()
        {
            var handlerCounter = new Counter();

            var server = Init(services =>
            {
                // Flow which moves the events from local to flowoutput channel
                services.RegisterEventFlow<FirstCustomTestFlow>();
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
            var manager = server.GetRequiredService<ICloudEventFlowManager>();

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
                services.RegisterEventFlow<ConfigurationFlow>();
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
            var manager = server.GetRequiredService<ICloudEventFlowManager>();

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
