using System;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.Components
{
    public class ChannelComponent : CloudEventsComponent
    {
        public ChannelComponent(IChannel channel, Predicate<CloudEvent> predicate)
        {
            Func = async ev =>
            {
                await channel.Send(ev);

                return ev;
            };

            Predicate = predicate ?? (ev => true);
        }
    }
}
