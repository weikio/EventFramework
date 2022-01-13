using System;
using System.Threading.Tasks;
using EventFrameworkTestBed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.EventFlow.CloudEvents;
using Weikio.EventFramework.IntegrationTests.EventFlow.ComponentsHandlers;
using Weikio.EventFramework.IntegrationTests.EventSource.Sources;
using Weikio.EventFramework.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.EventFramework.IntegrationTests.EventFlow
{
    public class MarkdownTests : IntegrationFlowTestBase
    {
        public MarkdownTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact]
        public async Task CanOutputIntegrationFlowAsMermaid()
        {
            var server = Init();
            var logger = server.GetRequiredService<ILogger<MarkdownTests>>();

            var handlerCounter = new Counter();

            var flowBuilder = EventFlowBuilder.From<NumberEventSource>()
                .Channel("hellochannel")
                .Channel("specialchannel", ev => ev.Type == "special")
                .Transform(ev =>
                {
                    ev.Subject = "transformed";

                    return ev;
                })
                .Branch((ev =>
                {
                    return ev.To<CounterEvent>().Object.CurrentCount == 1;
                }, branch =>
                {
                    branch
                        .Filter(ev => Filter.Continue)
                        .Handle(ev =>
                        {
                            logger.LogInformation("branch");
                        });
                }))
                .Filter(ev => ev.Type != "CounterEvent")
                .Flow("MySub")
                .Handle<FlowHandler>(configure: handler =>
                {
                    handler.Counter = handlerCounter;
                });

            var flow = await flowBuilder.Build(server);

            var subFlowBuilder = EventFlowBuilder.From()
                .WithDefinition("MySub")
                .Handle(ev =>
                {
                    logger.LogInformation(ev.ToJson());
                });

            var manager = server.GetRequiredService<ICloudEventFlowManager>();
            await manager.Execute(flow);

            var subflow = await subFlowBuilder.Build(server);
            await manager.Execute(subflow);

            var channels = server.GetRequiredService<IChannelManager>().Channels;

            logger.LogInformation("Channels:");

            foreach (var channel in channels)
            {
                logger.LogInformation(channel.Name);
            }

            logger.LogInformation("Components:");

            foreach (var component in flow.Components)
            {
                logger.LogInformation(component.ToString());
            }

            logger.LogInformation("Source:");

            foreach (var channel in channels)
            {
                logger.LogInformation(channel.Name);
            }
        }
    }
}
