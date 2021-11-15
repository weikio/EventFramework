using System;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Extensions;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Components;
using Weikio.EventFramework.EventSource.SDK;
using Weikio.EventFramework.IntegrationFlow;
using Weikio.EventFramework.IntegrationFlow.CloudEvents;
using Weikio.EventFramework.IntegrationTests.EventSource.Sources;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.IntegrationFlow
{
    public class CloudEventsIntegrationFlowBuilderTests : IntegrationFlowTestBase
    {
        public CloudEventsIntegrationFlowBuilderTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public void CanCreateFlowBuilderUsingResourceName()
        {
            var server = Init();

            // var flowBuilder = IntegrationFlowBuilder.From("hello");
        }

        [Fact]
        public async Task CanCreateFlowBuilderUsingNewEventSource()
        {
            var server = Init();
            var handlerCounter = new Counter();

            var flowBuilder = IntegrationFlowBuilder.From<NumberEventSource>()
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

            var manager = server.GetRequiredService<ICloudEventsIntegrationFlowManager>();
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

            var flowBuilder = IntegrationFlowBuilder.From("mynumberflow")
                .Handle(ev =>
                {
                    msgReceived = true;
                });

            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventsIntegrationFlowManager>();
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

            var flowBuilder = IntegrationFlowBuilder.From("testchannel")
                .Handle(ev =>
                {
                    msgReceived = true;
                });

            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventsIntegrationFlowManager>();
            await manager.Execute(flow);

            var channelManager = server.GetRequiredService<IChannelManager>();
            var testChannel = channelManager.Get("testchannel");

            await testChannel.Send(new CustomerCreatedEvent());

            await ContinueWhen(() => msgReceived, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void CanCreateFlowUsingExistingIntegrationFlow()
        {
            throw new NotImplementedException();
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

            var flowBuilder = IntegrationFlowBuilder.From("testchannel");
            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventsIntegrationFlowManager>();

            await Assert.ThrowsAsync<NotSupportedChannelTypeForIntegrationFlow>(async () =>
            {
                await manager.Execute(flow);
            });
        }

        [Fact]
        public async Task ThrowsSourceUnknownIfNoSuitableSourceFound()
        {
            var server = Init();

            var flowBuilder = IntegrationFlowBuilder.From("unknown");
            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventsIntegrationFlowManager>();

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

            var flowBuilder = IntegrationFlowBuilder.From<NumberEventSource>()
                .Handle(ev =>
                {
                    extensionFound = ev.GetAttributes().ContainsKey(EventFrameworkIntegrationFlowEventExtension.EventFrameworkIntegrationFlowAttributeName);
                });

            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventsIntegrationFlowManager>();
            await manager.Execute(flow);

            await ContinueWhen(() => extensionFound, timeout: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CanCreateFlowBuilderInConfigureServices()
        {
            var handlerCounter = new Counter();

            Init(services =>
            {
                services.AddIntegrationFlow(IntegrationFlowBuilder.From<NumberEventSource>()
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

                services.AddIntegrationFlow(IntegrationFlowBuilder.From<NumberEventSource>(options =>
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

                services.AddIntegrationFlow(IntegrationFlowBuilder.From("local")
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

                services.AddIntegrationFlow<FirstCustomTestFlow>();
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

                services.AddIntegrationFlow<CustomTestFlow>(flow =>
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

                services.AddIntegrationFlow<DependencyTestFlow>();
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

                services.AddIntegrationFlow<ConfigurationFlow>(new ConfigurationFlow.Config() { TargetChannelName = "testconfig" });
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

                services.AddIntegrationFlow<ConfigurationFlow>(new ConfigurationFlow.Config() { TargetChannelName = "testconfig" });

                services.AddIntegrationFlow<ConfigurationFlow>(new ConfigurationFlow.Config() { TargetChannelName = "anothertestconfig" });
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
                services.AddIntegrationFlow(IntegrationFlowBuilder.From<NumberEventSource>(options =>
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

        public class FirstCustomTestFlow : CloudEventsIntegrationFlowBase
        {
            public FirstCustomTestFlow()
            {
                Flow = IntegrationFlowBuilder.From("local")
                    .Channel("flowoutput");
            }
        }

        public class DependencyTestFlow : CloudEventsIntegrationFlowBase
        {
            private readonly ILogger<DependencyTestFlow> _logger;
            private readonly Counter _handlerCounter;

            public DependencyTestFlow(ILogger<DependencyTestFlow> logger, Counter handlerCounter)
            {
                _logger = logger;
                _handlerCounter = handlerCounter;

                Flow = IntegrationFlowBuilder.From("local")
                    .Handle(ev =>
                    {
                        _handlerCounter.Increment();
                    });
            }
        }

        public class CustomTestFlow : CloudEventsIntegrationFlowBase
        {
            public Counter HandlerCounter;

            public CustomTestFlow()
            {
                Flow = IntegrationFlowBuilder.From("local")
                    .Handle(ev =>
                    {
                        HandlerCounter.Increment();
                    });
            }
        }

        public class ConfigurationFlow : CloudEventsIntegrationFlowBase
        {
            public ConfigurationFlow(Config config)
            {
                Flow = IntegrationFlowBuilder.From("local")
                    .Channel(config.TargetChannelName);
            }

            public class Config
            {
                public string TargetChannelName { get; set; }
            }
        }

        public class FlowHandler
        {
            public Counter Counter { get; set; }

            public Task Handle(CloudEvent ev)
            {
                Counter.Increment();

                return Task.CompletedTask;
            }
        }

        public class Counter
        {
            private int _count;

            public void Increment()
            {
                Interlocked.Increment(ref _count);
            }

            public int Get()
            {
                return _count;
            }
        }
    }
}
