using System;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Components;
using Weikio.EventFramework.EventAggregator.Core;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public static class CloudEventsChannelBuilderExtensions
    {
        public static CloudEventsChannelBuilder EventAggregator(this CloudEventsChannelBuilder builder)
        {
            builder.Component(new EventAggregatorComponentBuilder());

            return builder;
        }
    }
    public class EventAggregatorComponentBuilder : IComponentBuilder
    {
        public Task<CloudEventsComponent> Build(ComponentFactoryContext context)
        {
            var aggr = context.ServiceProvider.GetRequiredService<ICloudEventAggregator>();

            var result = new CloudEventsComponent(async ev =>
            {
                await aggr.Publish(ev);

                return ev;
            });

            return Task.FromResult(result);
        }
    }

    public class ChannelComponentBuilder : IComponentBuilder
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
}
