using System;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventFlow.CloudEvents;

namespace Weikio.EventFramework.Components
{
    public static class CloudEventsChannelBuilderExtensions
    {
        public static CloudEventsChannelBuilder EventAggregator(this CloudEventsChannelBuilder builder)
        {
            builder.Component(new EventAggregatorComponentBuilder());

            return builder;
        }

        public static CloudEventsChannelBuilder Channel(this CloudEventsChannelBuilder builder, string channelName, Predicate<CloudEvent> predicate = null,
            bool autoCreateChannel = false)
        {
            var component = new ChannelComponentBuilder(channelName, predicate, autoCreateChannel);

            builder.Component(component);

            return builder;
        }
    }
}
