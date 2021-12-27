using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventFlow;
using Weikio.EventFramework.EventFlow.CloudEvents;
using Weikio.EventFramework.EventSource.SDK;
using Weikio.EventFramework.IntegrationTests.EventFlow.ComponentsHandlers;
using Weikio.EventFramework.IntegrationTests.EventFlow.Flows;
using Weikio.EventFramework.IntegrationTests.EventSource.Sources;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.EventFlow
{
    public class CloudEventFlowBuilderTests : IntegrationFlowTestBase
    {
        public CloudEventFlowBuilderTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public void CanCreateFlowBuilderUsingResourceName()
        {
            var server = Init();

            // var flowBuilder = IntegrationFlowBuilder.From("hello");
        }

        [Fact]
        public async Task CanCreateFlowWithoutSource()
        {
            var counter = new Counter();

            var server = Init();

            var flowBuilder = EventFlowBuilder.From()
                .Handle(ev => counter.Increment());

            var flow = await flowBuilder.Build(server);
            var manager = server.GetRequiredService<ICloudEventFlowManager>();

            await manager.Execute(flow);
        }

        [Fact]
        public async Task CanCreateFlowBuilderUsingNewEventSource()
        {
            var server = Init();
            var handlerCounter = new Counter();

            var flowBuilder = EventFlowBuilder.From<NumberEventSource>()
                .Channel("hellochannel")
                .Channel("specialchannel", ev => ev.Type == "special")
                .Transform(ev =>
                {
                    ev.Subject = "transformed";

                    return ev;
                })
                .Filter(ev => ev.Type != "CounterEvent")
                .Handle<FlowHandler>(configure: handler =>
                {
                    handler.Counter = handlerCounter;
                });

            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventFlowManager>();
            await manager.Execute(flow);

            await ContinueWhen(() => handlerCounter.Get() > 0, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CanCreateFlowUsingExistingEventSource()
        {
            var server = Init(services =>
            {
                services.AddEventSource<NumberEventSource>(options =>
                {
                    options.Autostart = true;
                    options.Id = "mynumberflow";
                    options.PollingFrequency = TimeSpan.FromSeconds(1);
                });
            });

            var msgReceived = false;

            var flowBuilder = EventFlowBuilder.From("mynumberflow")
                .Handle(ev =>
                {
                    msgReceived = true;
                });

            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventFlowManager>();
            await manager.Execute(flow);

            await ContinueWhen(() => msgReceived, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CanCreateFlowUsingExistingChannel()
        {
            var server = Init(services =>
            {
                services.AddChannel("testchannel");
            });

            var msgReceived = false;

            var flowBuilder = EventFlowBuilder.From("testchannel")
                .Handle(ev =>
                {
                    msgReceived = true;
                });

            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventFlowManager>();
            await manager.Execute(flow);

            var channelManager = server.GetRequiredService<IChannelManager>();
            var testChannel = channelManager.Get("testchannel");

            await testChannel.Send(new CustomerCreatedEvent());

            await ContinueWhen(() => msgReceived, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CanCreateFlowUsingExistingEventFlowAsSource()
        {
            var sourceFlow = EventFlowBuilder.From("local")
                .WithName("source")
                .Channel("test");

            var msgReceived = false;

            var targetFlow = EventFlowBuilder.From("source")
                .Handle(ev =>
                {
                    msgReceived = true;
                });

            var server = Init(services =>
            {
                services.AddChannel("local");
                services.AddEventFlow(sourceFlow);
                services.AddEventFlow(targetFlow);
            });

            var testChannel = server.GetRequiredService<IChannelManager>().Get("local");

            await testChannel.Send(new CustomerCreatedEvent());

            await ContinueWhen(() => msgReceived, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CanCreateSourceChannelAutomatically()
        {
            var server = Init();

            var manager = server.GetRequiredService<ICloudEventFlowManager>();

            var sourceFlow = await EventFlowBuilder.From("local")
                .Channel("test")
                .Build(server);

            // Should not throw
            await manager.Execute(sourceFlow);
        }

        [Fact]
        public async Task FlowRequiresPubSubChannel()
        {
            var server = Init(services =>
            {
                services.AddChannel("testchannel", (provider, options) =>
                {
                    options.IsPubSub = false;
                });
            });

            var flowBuilder = EventFlowBuilder.From("testchannel");
            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventFlowManager>();

            await Assert.ThrowsAsync<NotSupportedChannelTypeForEventFlow>(async () =>
            {
                await manager.Execute(flow);
            });
        }

        [Fact]
        public async Task ThrowsSourceUnknownIfNoSuitableSourceFound()
        {
            var server = Init();

            var flowBuilder = EventFlowBuilder.From("unknown");
            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventFlowManager>();

            await Assert.ThrowsAsync<UnknownIntegrationFlowSourceException>(async () =>
            {
                await manager.Execute(flow);
            });
        }

        [Fact]
        public async Task IntegrationFlowExtensionIsAddedToEvent()
        {
            var server = Init();

            var extensionFound = false;

            var flowBuilder = EventFlowBuilder.From<NumberEventSource>()
                .Handle(ev =>
                {
                    extensionFound = ev.GetAttributes().ContainsKey(EventFrameworkEventFlowEventExtension.EventFrameworkEventFlowAttributeName);
                });

            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventFlowManager>();
            await manager.Execute(flow);

            await ContinueWhen(() => extensionFound, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task EventFlowHasOutputChannel()
        {
            var server = Init();

            // Create flow
            var flowBuilder = EventFlowBuilder.From<NumberEventSource>(options =>
            {
                options.Autostart = true;
                options.PollingFrequency = TimeSpan.FromSeconds(1);
            });

            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventFlowManager>();

            // Run the flow
            var runningFlow = await manager.Execute(flow);

            // Create a new channel
            var received = false;

            var testChannel = new CloudEventsChannel("test", ev =>
            {
                received = true;
            });

            server.GetRequiredService<ICloudEventsChannelManager>().Add(testChannel);

            // Subscribe new channel to flow's output channel
            var outputChannelName = runningFlow.OutputChannel;
            var outputChannel = server.GetRequiredService<ICloudEventsChannelManager>().Get(outputChannelName);
            outputChannel.Subscribe(testChannel);

            await ContinueWhen(() => received, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task EventFlowOutputsTheFlowsEndResult()
        {
            var server = Init();

            // Create flow
            var flowBuilder = EventFlowBuilder.From<NumberEventSource>(options =>
            {
                options.Autostart = true;
                options.PollingFrequency = TimeSpan.FromSeconds(1);
            }).Transform(ev =>
            {
                var e = CloudEventCreator.Create(new CustomerCreatedEvent());

                return e;
            });

            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventFlowManager>();

            // Run the flow
            var runningFlow = await manager.Execute(flow);

            // Create a new channel
            CloudEvent receivedEv = null;

            var testChannel = new CloudEventsChannel("test", ev =>
            {
                receivedEv = ev;
            });

            server.GetRequiredService<ICloudEventsChannelManager>().Add(testChannel);

            // Subscribe new channel to flow's output channel
            var outputChannelName = runningFlow.OutputChannel;
            var outputChannel = server.GetRequiredService<ICloudEventsChannelManager>().Get(outputChannelName);
            outputChannel.Subscribe(testChannel);

            await ContinueWhen(() => receivedEv != null, timeout: TimeSpan.FromSeconds(5));
            Assert.Equal("CustomerCreatedEvent", receivedEv.Type);
        }

        [Fact]
        public async Task CanCreateFlowBuilderInConfigureServices()
        {
            var handlerCounter = new Counter();

            Init(services =>
            {
                services.AddEventFlow(EventFlowBuilder.From<NumberEventSource>()
                    .Channel("hellochannel")
                    .Channel("specialchannel", ev => ev.Type == "special")
                    .Transform(ev =>
                    {
                        ev.Subject = "transformed";

                        return ev;
                    })
                    .Filter(ev => ev.Type != "CounterEvent")
                    .Handle<FlowHandler>(configure: handler =>
                    {
                        handler.Counter = handlerCounter;
                    }));
            });

            await ContinueWhen(() => handlerCounter.Get() > 0, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task FilteringBreaksTheFlowIfWanted()
        {
            var handlerCounter = new Counter();

            Init(services =>
            {
                var received = false;

                services.AddEventFlow(EventFlowBuilder.From<NumberEventSource>(options =>
                    {
                        options.PollingFrequency = TimeSpan.FromSeconds(1);
                        options.Autostart = true;
                    })
                    .Filter(ev =>
                    {
                        if (received)
                        {
                            return Filter.Skip;
                        }

                        received = true;

                        return Filter.Continue;
                    })
                    .Handle<FlowHandler>(configure: handler =>
                    {
                        handler.Counter = handlerCounter;
                    }));
            });

            await Task.Delay(TimeSpan.FromSeconds(5));

            Assert.Equal(1, handlerCounter.Get());
        }

        [Fact]
        public async Task CanAccessOtherResourcesInFlowsCreatedInConfigureServices()
        {
            var handlerCounter = new Counter();

            var provider = Init(services =>
            {
                services.AddChannel("local");

                services.AddEventFlow(EventFlowBuilder.From("local")
                    .Handle<FlowHandler>(configure: handler =>
                    {
                        handler.Counter = handlerCounter;
                    }));
            });

            var channel = provider.GetRequiredService<IChannelManager>().Get("local");

            await channel.Send(new CustomerCreatedEvent());

            await ContinueWhen(() => handlerCounter.Get() > 0, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CanCreateFlowByInheritingBaseClass()
        {
            var handlerCounter = new Counter();

            var provider = Init(services =>
            {
                services.AddChannel("local");

                services.AddChannel("flowoutput", (serviceProvider, options) =>
                {
                    options.Endpoint = ev =>
                    {
                        handlerCounter.Increment();
                    };
                });

                services.AddEventFlow<FirstCustomTestFlow>();
            });

            var channel = provider.GetRequiredService<IChannelManager>().Get("local");

            await channel.Send(new CustomerCreatedEvent());

            await ContinueWhen(() => handlerCounter.Get() > 0, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CanConfigureFlow()
        {
            var handlerCounter = new Counter();

            var provider = Init(services =>
            {
                services.AddChannel("local");

                services.AddEventFlow<CustomTestFlow>(flow =>
                {
                    flow.HandlerCounter = handlerCounter;
                });
            });

            var channel = provider.GetRequiredService<IChannelManager>().Get("local");

            await channel.Send(new CustomerCreatedEvent());

            await ContinueWhen(() => handlerCounter.Get() > 0, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CanInjectDependenciesInFlow()
        {
            var handlerCounter = new Counter();

            var provider = Init(services =>
            {
                services.AddChannel("local");
                services.AddSingleton(handlerCounter);

                services.AddEventFlow<DependencyTestFlow>();
            });

            var channel = provider.GetRequiredService<IChannelManager>().Get("local");

            await channel.Send(new CustomerCreatedEvent());

            await ContinueWhen(() => handlerCounter.Get() > 0, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CanUseConfiguration()
        {
            var counter = new Counter();

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

                services.AddEventFlow<ConfigurationFlow>(new ConfigurationFlow.Config() { TargetChannelName = "testconfig" });
            });

            var channel = provider.GetRequiredService<IChannelManager>().Get("local");
            await channel.Send(new CustomerCreatedEvent());

            await ContinueWhen(() => counter.Get() > 0, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CanHaveMultipleInstancesOfFlowWithDifferentConfigurations()
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

            var channel = provider.GetRequiredService<IChannelManager>().Get("local");
            await channel.Send(new CustomerCreatedEvent());

            await ContinueWhen(() => counter.Get() > 0 && secondCounter.Get() > 0, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task FlowReceivesLatestEvent()
        {
            var currentValue = -1;

            Init(services =>
            {
                services.AddEventFlow(EventFlowBuilder.From<NumberEventSource>(options =>
                    {
                        options.Autostart = true;
                        options.PollingFrequency = TimeSpan.FromSeconds(1);
                    })
                    .Transform(ev =>
                    {
                        currentValue = ev.To<CounterEvent>().Object.CurrentCount;

                        return ev;
                    }));
            });

            await ContinueWhen(() => currentValue > 2, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CanExecuteRegisteredIntegrationFlowRuntime()
        {
            var counter = new Counter();

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

                services.AddEventFlow<ConfigurationFlow>(new ConfigurationFlow.Config() { TargetChannelName = "testconfig" });
            });

            var channel = provider.GetRequiredService<IChannelManager>().Get("local");
            await channel.Send(new CustomerCreatedEvent());

            await ContinueWhen(() => counter.Get() > 0, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CanTransferToChannelAfterFlow()
        {
            var testChannelCounter = new Counter();
            var anotherTestChannelCounter = new Counter();

            var server = Init(collection =>
            {
                collection.AddChannel("input");

                collection.AddChannel("test", (provider, options) =>
                {
                    options.Endpoint = ev => testChannelCounter.Increment();
                });

                collection.AddChannel("anothertest", (provider, options) =>
                {
                    options.Endpoint = ev => anotherTestChannelCounter.Increment();
                });
            });

            var handlerCounter = new Counter();

            var flowBuilder = EventFlowBuilder.From("input")
                .Handle(ev =>
                {
                    handlerCounter.Increment();
                });

            var flow = await flowBuilder.Build(server);
            var manager = server.GetRequiredService<ICloudEventFlowManager>();
            await manager.Execute(flow);

            var inputChannel = server.GetRequiredService<IChannelManager>().Get("input");

            var ev1 = CloudEventCreator.Create(new CounterEvent(),
                extensions: new ICloudEventExtension[] { new EventFrameworkEventFlowEndpointEventExtension("test") });

            var ev2 = CloudEventCreator.Create(new CounterEvent(),
                extensions: new ICloudEventExtension[] { new EventFrameworkEventFlowEndpointEventExtension("test") });

            var ev3 = CloudEventCreator.Create(new CounterEvent(),
                extensions: new ICloudEventExtension[] { new EventFrameworkEventFlowEndpointEventExtension("anothertest") });
            var ev4 = CloudEventCreator.Create(new CounterEvent());

            await inputChannel.Send(ev1);
            await inputChannel.Send(ev2);
            await inputChannel.Send(ev3);
            await inputChannel.Send(ev4);

            await ContinueWhen(() => testChannelCounter.Get() > 0 && anotherTestChannelCounter.Get() > 0 && handlerCounter.Get() == 4,
                timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CanHandleMessagesInOrder()
        {
            var server = Init();
            var handlerCounter = new Counter();
            var msgCount = 0;

            var flowBuilder = EventFlowBuilder.From<NumberEventSource>()
                .Transform(ev =>
                {
                    msgCount += 1;

                    return ev;
                })
                .Handle(ev =>
                {
                    handlerCounter.Increment();
                })
                .Transform(ev => ev)
                .Handle(ev =>
                {
                    handlerCounter.Increment();
                });

            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventFlowManager>();
            await manager.Execute(flow);

            await ContinueWhen(() => msgCount == 1, timeout: TimeSpan.FromSeconds(5));

            Assert.Equal(2, handlerCounter.Get());
        }
    }
}
