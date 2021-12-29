using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventAggregator.Core;

namespace Weikio.EventFramework.EventFlow.CloudEvents.Components
{
    public class HandlerComponentBuilder : IComponentBuilder
    {
        private readonly Func<CloudEvent, IServiceProvider, Task> _handler;
        private readonly Type _handlerType;
        private readonly MulticastDelegate _configureHandler;
        private readonly Func<CloudEvent, Task<bool>> _predicate;

        public HandlerComponentBuilder(Func<CloudEvent, IServiceProvider, Task> handler,
            Func<CloudEvent, Task<bool>> predicate = null, Type handlerType = null, MulticastDelegate configureHandler = null)
        {
            _handler = handler;
            _handlerType = handlerType;
            _configureHandler = configureHandler;
            _predicate = predicate ?? (ev => Task.FromResult(true));
        }

        public Task<CloudEventsComponent> Build(ComponentFactoryContext context)
        {
            var eventLinkInitializer = context.ServiceProvider.GetRequiredService<EventLinkInitializer>();
            var typeToEventLinksConverter = context.ServiceProvider.GetRequiredService<ITypeToEventLinksConverter>();

            var predicate = _predicate;
            if (predicate == null)
            {
                predicate = cloudEvent => Task.FromResult(true);
            }

            predicate = predicate + (ev =>
            {
                var attrs = ev.GetAttributes();

                if (attrs.ContainsKey(EventFrameworkEventFlowEventExtension.EventFrameworkEventFlowAttributeName) == false)
                {
                    return Task.FromResult(false);
                }

                if (attrs.ContainsKey(EventFrameworkEventFlowCurrentChanneEventExtension.EventFrameworkEventFlowCurrentChannelAttributeName) == false)
                {
                    return Task.FromResult(false);
                }

                var channelId = attrs[EventFrameworkEventFlowCurrentChanneEventExtension.EventFrameworkEventFlowCurrentChannelAttributeName] as string;

                return Task.FromResult(string.Equals(context.ChannelName, channelId));
            });

            if (_handlerType != null)
            {
                var links = typeToEventLinksConverter.Create(context.ServiceProvider, _handlerType, predicate, _configureHandler);

                foreach (var eventLink in links)
                {
                    eventLinkInitializer.Initialize(eventLink);
                }
            }

            if (_handler != null)
            {
                var link = new EventLink(predicate, _handler);
                eventLinkInitializer.Initialize(link);
            }

            var aggregatorComponent = new CloudEventsComponent(async ev =>
            {
                var aggr = context.ServiceProvider.GetRequiredService<ICloudEventAggregator>();
                await aggr.Publish(ev);

                return ev;
            });

            return Task.FromResult(aggregatorComponent);
        }
    }
}
