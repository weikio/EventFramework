using System;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.IntegrationFlow.CloudEvents;
using Weikio.EventFramework.IntegrationTests.EventSource.Sources;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.IntegrationFlow
{
    public class SubflowTests : IntegrationFlowTestBase
    {
        public SubflowTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task CanRunSubflowWithName()
        {
            throw new NotImplementedException();
            // var server = Init();
            // var handlerCounter = new Counter();
            // var subflowCounter = new Counter();
            //
            // var subflowBuilder = IntegrationFlowBuilder.From()
            //     .WithId("mysub")
            //     .Handle(ev =>
            //     {
            //         subflowCounter.Increment();
            //     });
            //
            // var flowBuilder = IntegrationFlowBuilder.From<NumberEventSource>()
            //     .Flow("mysub")
            //     .Transform(ev =>
            //     {
            //         return ev;
            //     })
            //     .Handle(ev =>
            //     {
            //         handlerCounter.Increment();
            //     });
            //
            // var subflow = await subflowBuilder.Build(server);
            // var flow = await flowBuilder.Build(server);
            //
            // var manager = server.GetRequiredService<ICloudEventsIntegrationFlowManager>();
            //
            // await manager.Execute(subflow);
            // await manager.Execute(flow);
            //
            // await ContinueWhen(() => handlerCounter.Get() > 0, timeout: TimeSpan.FromSeconds(5));
            //
            // Assert.Equal(handlerCounter.Get(), subflowCounter.Get());
        }

        [Fact]
        public async Task CanRunSubflowWithNameAndPredicate()
        {
            throw new NotImplementedException();
            var server = Init(services =>
            {
                services.AddChannel("local");
            });

            var handlerCounter = new Counter();
            var subflowCounter = new Counter();

            var subflowBuilder = IntegrationFlowBuilder.From()
                .WithName("mysub")
                .Handle(ev =>
                {
                    subflowCounter.Increment();
                });

            var flowBuilder = IntegrationFlowBuilder.From("local")
                .Flow("mysub", ev =>
                {
                    var number = ev.To<CounterEvent>().Object.CurrentCount;

                    if ((decimal)number % 2 == 0)
                    {
                        return true;
                    }

                    return false;
                })
                .Transform(ev =>
                {
                    return ev;
                })
                .Handle(ev =>
                {
                    handlerCounter.Increment();
                });

            var subflow = await subflowBuilder.Build(server);
            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventsIntegrationFlowManager>();

            await manager.Execute(subflow);
            await manager.Execute(flow);

            var localChannel = server.GetRequiredService<IChannelManager>().Get("local");

            for (var i = 0; i < 10; i++)
            {
                var counterEvent = new CounterEvent() { CurrentCount = i };

                await localChannel.Send(counterEvent);
            }

            await ContinueWhen(() => handlerCounter.Get() == 10, timeout: TimeSpan.FromSeconds(5));

            Assert.Equal(5, subflowCounter.Get());
        }

        [Fact]
        public async Task CanBuildSubflow()
        {
            var server = Init();
            var handlerCounter = new Counter();
            var subflowCounter = new Counter();

            var flowBuilder = IntegrationFlowBuilder.From<NumberEventSource>()
                .Flow(builder =>
                {
                    builder.Handle(ev =>
                    {
                        subflowCounter.Increment();
                    });
                })
                .Handle(ev =>
                {
                    handlerCounter.Increment();
                });

            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventsIntegrationFlowManager>();

            await manager.Execute(flow);

            await ContinueWhen(() => handlerCounter.Get() > 0, timeout: TimeSpan.FromSeconds(5));

            Assert.Equal(handlerCounter.Get(), subflowCounter.Get());
        }

        [Fact]
        public async Task SubflowReturnsToMainFlow()
        {
            var server = Init();
            var handlerCounter = new Counter();
            
            var flowBuilder = IntegrationFlowBuilder.From<NumberEventSource>()
                .Flow(builder =>
                {
                })
                .Handle<FlowHandler>(configure: handler =>
                {
                    handler.Counter = handlerCounter;
                });
            
            var flow = await flowBuilder.Build(server);
            
            var manager = server.GetRequiredService<ICloudEventsIntegrationFlowManager>();
            await manager.Execute(flow);
            
            await ContinueWhen(() => handlerCounter.Get() > 0, timeout: TimeSpan.FromSeconds(5));
        }
    }
}
