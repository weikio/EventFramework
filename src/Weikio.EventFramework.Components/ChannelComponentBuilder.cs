using System;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Components;

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
}