using System;
using System.Collections.Generic;
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
        private readonly Action<IEventFlowBuilder> _buildFlow;
        private readonly EventFlowDefinition _flowDefinition;
        private readonly Predicate<CloudEvent> _predicate;
        private readonly string _flowId;

        public FlowComponentBuilder(Action<IEventFlowBuilder> buildFlow, Predicate<CloudEvent> predicate = null)
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

            var nextChannel = context.Tags["nextchannelname"].FirstOrDefault();
            var hasNext = nextChannel != default;

            if (context?.Tags?.Any(x => x.Key == "step") == true)
            {
                var step = (Step)context.Tags["step"].FirstOrDefault().Value;

                var subflowDescriptor = flowId;

                if (string.IsNullOrWhiteSpace(subflowDescriptor))
                {
                    subflowDescriptor = _flowDefinition.ToString();
                }

                step.Links[0] = new StepLink(StepLinkType.Subflow, subflowDescriptor);

                if (hasNext)
                {
                    var steps = (List<Step>) context.Tags["steps"].FirstOrDefault().Value;
                    steps.Add(new Step(subflowDescriptor, new StepLink(StepLinkType.Channel, flowId)));
                }
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
