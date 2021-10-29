using System;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions;
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

            var manager = server.GetRequiredService<CloudEventsIntegrationFlowManager>();
            await manager.Execute(flow);

            await ContinueWhen(() => handlerCounter.Get() > 0, timeout: TimeSpan.FromSeconds(15));
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
