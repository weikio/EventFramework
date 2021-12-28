using System;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventFlow.Abstractions;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class FlowComponentBuilder
    {
        public Func<ComponentFactoryContext, Task<CloudEventsComponent>> Build(Action<EventFlowBuilder> buildFlow, Predicate<CloudEvent> predicate = null)
        {
            return CreateComponentBuilder(null, buildFlow, predicate);
        }

        public Func<ComponentFactoryContext, Task<CloudEventsComponent>> Build(EventFlowDefinition flowDefinition = null,
            Predicate<CloudEvent> predicate = null, string flowId = null)
        {
            return CreateComponentBuilder(flowDefinition, null, predicate, flowId);
        }

        private Func<ComponentFactoryContext, Task<CloudEventsComponent>> CreateComponentBuilder(EventFlowDefinition flowDefinition = null,
            Action<EventFlowBuilder> buildFlow = null, Predicate<CloudEvent> predicate = null, string flowId = null)
        {
            async Task<CloudEventsComponent> Handler(ComponentFactoryContext context)
            {
                var instanceManager = context.ServiceProvider.GetRequiredService<ICloudEventFlowManager>();
                var channelManager = context.ServiceProvider.GetRequiredService<IChannelManager>();

                if (buildFlow != null)
                {
                    var flowBuilder = EventFlowBuilder.From();
                    buildFlow(flowBuilder);

                    flowId = $"{Guid.NewGuid()}";

                    var subflow = await flowBuilder.Build(context.ServiceProvider);

                    await instanceManager.Execute(subflow, new EventFlowInstanceOptions() { Id = flowId });
                }

                var flowComponent = new CloudEventsComponent(async ev =>
                {
                    EventFlowInstance targetFlow;

                    if (!string.IsNullOrWhiteSpace(flowId))
                    {
                        targetFlow = instanceManager.Get(flowId);
                    }
                    else
                    {
                        var allFlows = instanceManager.List();
                        targetFlow = allFlows.FirstOrDefault(x => Equals(x.FlowDefinition, flowDefinition));
                    }

                    if (targetFlow == null)
                    {
                        throw new UnknownEventFlowInstance("", "Couldn't locate target flow using id or flow definition");
                    }

                    var nextChannel = context.Tags.FirstOrDefault(x => string.Equals(x.Key, "nextchannelname"));
                    var hasNext = nextChannel != default;

                    if (hasNext)
                    {
                        var nextInputChannel = nextChannel.Value.ToString();
                        var ext = new EventFrameworkEventFlowEndpointEventExtension(nextInputChannel);
                        ext.Attach(ev);
                    }

                    var targetFlowInputChannel = targetFlow.InputChannel;

                    var targetChannel = channelManager.Get(targetFlowInputChannel);
                    await targetChannel.Send(ev);

                    return null;
                }, predicate);

                return flowComponent;
            }

            return Handler;
        }
    }
}
