using System.Text;
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

            var sb = new StringBuilder();

            foreach (var channel in channels)
            {
                sb.AppendLine(channel.Name);
            }

            logger.LogInformation(sb.ToString());

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
        
        [Fact]
        public async Task CanOutputSimpleSteps()
        {
            var server = Init();
            var logger = server.GetRequiredService<ILogger<MarkdownTests>>();

            var handlerCounter = new Counter();

            var flowBuilder = EventFlowBuilder.From<NumberEventSource>()
                .Channel("hellochannel")
                .Channel("specialchannel")
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
            var instance = await manager.Execute(flow);

            var steps = instance.Steps;
            var sb = new StringBuilder();

            foreach (var step in steps)
            {
                foreach (var link in step.Links)
                {
                    sb.AppendLine($"{step.Id}-->|{link.Type}|{link.Id}");
                }
            }

            logger.LogInformation(sb.ToString());
        }

        [Fact]
        public async Task CanOutputPredicate()
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
                .Filter(ev => ev.Type != "CounterEvent")
                .Handle<FlowHandler>(configure: handler =>
                {
                    handler.Counter = handlerCounter;
                });

            var flow = await flowBuilder.Build(server);

            var manager = server.GetRequiredService<ICloudEventFlowManager>();
            var instance = await manager.Execute(flow);

            var steps = instance.Steps;
            var sb = new StringBuilder();

            foreach (var step in steps)
            {
                foreach (var link in step.Links)
                {
                    sb.AppendLine($"{step.Id}-->|{link.Type}|{link.Id}");
                }
            }

            logger.LogInformation(sb.ToString());
        }

        [Fact]
        public async Task CanOutputBranchSteps()
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
                .Handle<FlowHandler>(configure: handler =>
                {
                    handler.Counter = handlerCounter;
                });
        
            var flow = await flowBuilder.Build(server);
        
            var manager = server.GetRequiredService<ICloudEventFlowManager>();
            var instance = await manager.Execute(flow);
        
            var steps = instance.Steps;
            var sb = new StringBuilder();
        
            foreach (var step in steps)
            {
                foreach (var link in step.Links)
                {
                    sb.AppendLine($"{step.Id}-->|{link.Type}|{link.Id}");
                }
            }
        
            logger.LogInformation(sb.ToString());
        }
        //
        // [Fact]
        // public async Task CanOutputSubflowSteps()
        // {
        //     var server = Init();
        //     var logger = server.GetRequiredService<ILogger<MarkdownTests>>();
        //
        //     var handlerCounter = new Counter();
        //
        //     var flowBuilder = EventFlowBuilder.From<NumberEventSource>()
        //         .Channel("hellochannel")
        //         .Channel("specialchannel", ev => ev.Type == "special")
        //         .Transform(ev =>
        //         {
        //             ev.Subject = "transformed";
        //
        //             return ev;
        //         })
        //         .Filter(ev => ev.Type != "CounterEvent")
        //         .Flow("MySub")
        //         .Handle<FlowHandler>(configure: handler =>
        //         {
        //             handler.Counter = handlerCounter;
        //         });
        //
        //     var flow = await flowBuilder.Build(server);
        //
        //     var subFlowBuilder = EventFlowBuilder.From()
        //         .WithDefinition("MySub")
        //         .Handle(ev =>
        //         {
        //             logger.LogInformation(ev.ToJson());
        //         });
        //
        //     var manager = server.GetRequiredService<ICloudEventFlowManager>();
        //     var instance = await manager.Execute(flow);
        //
        //     var subflow = await subFlowBuilder.Build(server);
        //     await manager.Execute(subflow);
        //
        //     var steps = instance.Steps;
        //     var sb = new StringBuilder();
        //
        //     foreach (var step in steps)
        //     {
        //         foreach (var stepNext in step.Nexts)
        //         {
        //             sb.AppendLine($"{step.Current}-->{stepNext}");
        //         }
        //     }
        //
        //     logger.LogInformation(sb.ToString());
        // }
    }
}
