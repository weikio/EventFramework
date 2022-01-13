using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
}

namespace Weikio.EventFramework.EventFlow.CloudEvents.Components
{
    public class BranchComponentBuilder : IComponentBuilder
    {
        private readonly (Predicate<CloudEvent> Predicate, Action<EventFlowBuilder> BuildBranch)[] _branches;

        public BranchComponentBuilder(params (Predicate<CloudEvent> Predicate, Action<EventFlowBuilder> BuildBranch)[] branches)
        {
            _branches = branches;
        }

        public async Task<CloudEventsComponent> Build(ComponentFactoryContext context)
        {
            var instanceManager = context.ServiceProvider.GetRequiredService<ICloudEventFlowManager>();
            var channelManager = context.ServiceProvider.GetRequiredService<IChannelManager>();

            var createdFlows = new List<(Predicate<CloudEvent> Predicate, string ChannelId)>();

            var mainFlowId = context.Tags?.FirstOrDefault(x => x.Key == "flowid").Value ?? "";
            for (var index = 0; index < _branches.Length; index++)
            {
                var branchInstanceOptions = new EventFlowInstanceOptions() { Id = $"{mainFlowId}/branches/{index}" };

                // var branchChannelName = branchInstanceOptions.InputChannel;
                // var branchChannelOptions = new CloudEventsChannelOptions() { Name = branchChannelName };
                // var branchInputChannel = new CloudEventsChannel(branchChannelOptions);
                // channelManager.Add(branchInputChannel);

                var branch = _branches[index];
                var flowBuilder = EventFlowBuilder.From();
                branch.BuildBranch(flowBuilder);

                var branchFlow = await flowBuilder.Build(context.ServiceProvider);

                await instanceManager.Execute(branchFlow,
                    branchInstanceOptions);

                createdFlows.Add((branch.Predicate, branchInstanceOptions.InputChannel));
            }

            var branchComponent = new CloudEventsComponent(async ev =>
            {
                var branched = false;

                foreach (var createdFlow in createdFlows)
                {
                    var shouldBranch = createdFlow.Predicate(ev);

                    if (shouldBranch)
                    {
                        var channel = channelManager.Get(createdFlow.ChannelId);
                        await channel.Send(ev);

                        branched = true;
                    }
                }

                if (branched)
                {
                    return null;
                }

                return ev;
            });

            return branchComponent;
        }
    }
}
