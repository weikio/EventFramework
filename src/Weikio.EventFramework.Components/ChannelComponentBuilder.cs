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
    public class ChannelComponentBuilder : IComponentBuilder
    {
        private readonly string _channelName;
        private readonly Predicate<CloudEvent> _predicate;
        private readonly bool _autoCreateChannel;

        public ChannelComponentBuilder(string channelName, Predicate<CloudEvent> predicate, bool autoCreateChannel)
        {
            _channelName = channelName;
            _predicate = predicate;
            _autoCreateChannel = autoCreateChannel;
        }

        public Task<CloudEventsComponent> Build(ComponentFactoryContext context)
        {
            var channelManager = context.ServiceProvider.GetRequiredService<ICloudEventsChannelManager>();

            var result = new ChannelComponent(_channelName, s =>
            {
                var channel =
                    channelManager.Channels.FirstOrDefault(x => string.Equals(_channelName, x.Name, StringComparison.InvariantCultureIgnoreCase)) as
                        CloudEventsChannel;

                if (channel == null && _autoCreateChannel)
                {
                    channel = new CloudEventsChannel(_channelName);
                    channelManager.Add(channel);
                }

                return channel;
            }, _predicate);

            return Task.FromResult<CloudEventsComponent>(result);
        }
    }
}
