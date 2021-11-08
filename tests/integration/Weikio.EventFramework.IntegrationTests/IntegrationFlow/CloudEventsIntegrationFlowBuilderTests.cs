using System;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using EventFrameworkTestBed;
using EventFrameworkTestBed.Events;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
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
                .Filter(ev => ev.Type == "CounterEvent")
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
                    .Filter(ev => ev.Type == "CounterEvent")
                    .Handle<FlowHandler>(configure: handler =>
                    {
                        handler.Counter = handlerCounter;
                    }));
            });

            await ContinueWhen(() => handlerCounter.Get() > 0, timeout: TimeSpan.FromSeconds(5));
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

                services.AddIntegrationFlow<CustomTestFlow>(new Action<CustomTestFlow>(flow =>
                {
                    flow.HandlerCounter = handlerCounter;
                }));
            });

            var channel = provider.GetRequiredService<IChannelManager>().Get("local");

            await channel.Send(new CustomerCreatedEvent());
            
            await ContinueWhen(() => handlerCounter.Get() > 0, timeout: TimeSpan.FromSeconds(5));
        }

        public class FirstCustomTestFlow : CloudEventsIntegrationFlowBase
        {
            public FirstCustomTestFlow()
            {
                Flow = IntegrationFlowBuilder.From("local")
                    .Channel("flowoutput");
            }
        }
        
        public class CustomTestFlow : CloudEventsIntegrationFlowBase
        {
            public Counter HandlerCounter;

            public CustomTestFlow(Action<CustomTestFlow> configure) : base(configure)
            {
                Flow = IntegrationFlowBuilder.From("local")
                    .Handle(ev =>
                    {
                        HandlerCounter.Increment();
                    });
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
