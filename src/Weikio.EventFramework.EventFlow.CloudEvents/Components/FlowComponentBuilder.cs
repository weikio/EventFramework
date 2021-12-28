﻿using System;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventFlow.Abstractions;

namespace Weikio.EventFramework.EventFlow.CloudEvents.Components
{
    public class FlowComponentBuilder : IComponentBuilder
    {
        private readonly Action<EventFlowBuilder> _buildFlow;
        private readonly EventFlowDefinition _flowDefinition;
        private readonly Predicate<CloudEvent> _predicate;
        private readonly string _flowId;

        public FlowComponentBuilder(Action<EventFlowBuilder> buildFlow, Predicate<CloudEvent> predicate = null)
        {
            _buildFlow = buildFlow;
            _predicate = predicate;
        }

        public FlowComponentBuilder(EventFlowDefinition flowDefinition = null,
            Predicate<CloudEvent> predicate = null, string flowId = null)
        {
            _flowDefinition = flowDefinition;
            _predicate = predicate;
            _flowId = flowId;
        }

        public async Task<CloudEventsComponent> Build(ComponentFactoryContext context)
        {
            var instanceManager = context.ServiceProvider.GetRequiredService<ICloudEventFlowManager>();
            var channelManager = context.ServiceProvider.GetRequiredService<IChannelManager>();

            var flowId = _flowId;

            if (_buildFlow != null)
            {
                var flowBuilder = EventFlowBuilder.From();
                _buildFlow(flowBuilder);

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
                    targetFlow = allFlows.FirstOrDefault(x => Equals(x.FlowDefinition, _flowDefinition));
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
            }, _predicate);

            return flowComponent;
        }
    }
}
