using System;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventAggregator.Core;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public static class IntegrationFlowBuilderExtensions
    {
        public static IntegrationFlowBuilder Channel(this IntegrationFlowBuilder builder, string channelName, Predicate<CloudEvent> predicate = null)
        {
            Task<CloudEventsComponent> Handler(IServiceProvider provider)
            {
                var channelManager = provider.GetRequiredService<ICloudEventsChannelManager>();

                var channel =
                    channelManager.Channels.FirstOrDefault(x => string.Equals(channelName, x.Name, StringComparison.InvariantCultureIgnoreCase)) as
                        CloudEventsChannel;

                if (channel == null)
                {
                    channel = new CloudEventsChannel(channelName);
                    channelManager.Add(channel);
                }

                var result = new ChannelComponent(channel, predicate);

                return Task.FromResult<CloudEventsComponent>(result);
            }

            builder.Register(Handler);

            return builder;
        }

        public static IntegrationFlowBuilder Transform(this IntegrationFlowBuilder builder, Func<CloudEvent, CloudEvent> transform)
        {
            var component = new CloudEventsComponent(transform);
            builder.Register(component);

            return builder;
        }

        public static IntegrationFlowBuilder Filter(this IntegrationFlowBuilder builder, Predicate<CloudEvent> filter)
        {
            var component = new CloudEventsComponent(ev => ev, filter);
            builder.Register(component);

            return builder;
        }

        public static IntegrationFlowBuilder Handle<THandlerType>(this IntegrationFlowBuilder builder, Predicate<CloudEvent> predicate = null,
            Action<THandlerType> configure = null)
        {
            Task<CloudEventsComponent> Handler(IServiceProvider provider)
            {
                var typeToEventLinksConverter = provider.GetRequiredService<ITypeToEventLinksConverter>();
                var eventLinkInitializer = provider.GetRequiredService<EventLinkInitializer>();

                if (predicate == null)
                {
                    predicate = ev => true;
                }

                predicate = predicate + new Predicate<CloudEvent>(ev =>
                {
                    var attrs = ev.GetAttributes();

                    if (attrs.ContainsKey("eventFramework_eventsource") == false)
                    {
                        return false;
                    }

                    var flowId = attrs["eventFramework_eventsource"] as string;

                    return string.Equals(builder.Id, flowId);
                });
                
                var links = typeToEventLinksConverter.Create(provider, typeof(THandlerType), ev => Task.FromResult(predicate(ev)), configure);

                foreach (var eventLink in links)
                {
                    eventLinkInitializer.Initialize(eventLink);
                }

                var aggregatorComponent = new CloudEventsComponent(async ev =>
                {
                    var aggr = provider.GetRequiredService<ICloudEventAggregator>();
                    await aggr.Publish(ev);

                    return ev;
                });

                return Task.FromResult(aggregatorComponent);
            }

            builder.Register(Handler);

            return builder;
        }
    }
}
