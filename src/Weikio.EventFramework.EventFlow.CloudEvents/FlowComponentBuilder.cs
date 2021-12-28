using System;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Components;
using Weikio.EventFramework.EventFlow.Abstractions;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class ChannelComponentBuilder
    {
        private readonly string _channelName;
        private readonly Predicate<CloudEvent> _predicate;

        public ChannelComponentBuilder(string channelName, Predicate<CloudEvent> predicate)
        {
            _channelName = channelName;
            _predicate = predicate;
        }

        public Task<CloudEventsComponent> Build(ComponentFactoryContext context)
        {
            var channelManager = context.ServiceProvider.GetRequiredService<ICloudEventsChannelManager>();

            var channel =
                channelManager.Channels.FirstOrDefault(x => string.Equals(_channelName, x.Name, StringComparison.InvariantCultureIgnoreCase)) as
                    CloudEventsChannel;

            if (channel == null)
            {
                channel = new CloudEventsChannel(_channelName);
                channelManager.Add(channel);
            }

            var result = new ChannelComponent(channel, _predicate);

            return Task.FromResult<CloudEventsComponent>(result);
        }
    }

    public class FilterComponentBuilder
    {
        private readonly Func<CloudEvent, Filter> _filter;

        public FilterComponentBuilder(Predicate<CloudEvent> filter)
        {
            Filter Result(CloudEvent ev)
            {
                if (filter(ev))
                {
                    return Filter.Skip;
                }

                return Filter.Continue;
            }

            _filter = Result;
        }

        public FilterComponentBuilder(Func<CloudEvent, Filter> filter)
        {
            _filter = filter;
        }

        public Task<CloudEventsComponent> Build(ComponentFactoryContext context)
        {
            var component = new CloudEventsComponent(ev =>
            {
                if (_filter(ev) == Filter.Skip)
                {
                    return null;
                }

                return ev;
            });

            return Task.FromResult(component);
        }
    }

    public static class EventFlowBuilderFlowExtensions
    {
        public static EventFlowBuilder Flow(this EventFlowBuilder builder, Action<EventFlowBuilder> buildFlow,
            Predicate<CloudEvent> predicate = null)
        {
            var componentBuilder = new FlowComponentBuilder().Build(buildFlow, predicate);
            builder.Component(componentBuilder);

            return builder;
        }

        public static EventFlowBuilder Flow(this EventFlowBuilder builder, EventFlowDefinition flowDefinition,
            Predicate<CloudEvent> predicate = null, string flowId = null)
        {
            var componentBuilder = new FlowComponentBuilder().Build(flowDefinition, predicate, flowId);
            builder.Component(componentBuilder);

            return builder;
        }
    }

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
