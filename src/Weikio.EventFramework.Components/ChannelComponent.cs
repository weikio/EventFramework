using System;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.Components
{
    public class ChannelComponent : CloudEventsComponent
    {
        public ChannelComponent(string channelName, Func<string, IChannel> getChannel, Predicate<CloudEvent> predicate)
        {
            Func = async ev =>
            {
                var channel = getChannel(channelName);

                if (channel != null)
                {
                    await channel.Send(ev);
                }

                return ev;
            };

            Predicate = predicate ?? (ev => true);
        }
    }
}
