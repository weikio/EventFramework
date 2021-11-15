using System;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Components;
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

        public static IntegrationFlowBuilder Filter(this IntegrationFlowBuilder builder, Func<CloudEvent, Filter> filter)
        {
            var component = new CloudEventsComponent(ev =>
            {
                if (filter(ev) == CloudEvents.Filter.Skip)
                {
                    return null;
                }
                
                return ev;
            });
            
            builder.Register(component);

            return builder;
        }
        
        public static IntegrationFlowBuilder Filter(this IntegrationFlowBuilder builder, Predicate<CloudEvent> filter)
        {
            var component = new CloudEventsComponent(ev =>
            {
                if (filter(ev))
                {
                    return null;
                }
                
                return ev;
            });
            
            builder.Register(component);

            return builder;
        }

        public static IntegrationFlowBuilder Handle<THandlerType>(this IntegrationFlowBuilder builder, Predicate<CloudEvent> predicate = null,
            Action<THandlerType> configure = null)
        {
            if (predicate == null)
            {
                predicate = ev => true;
            }
            
            var taskPredicate = new Func<CloudEvent, Task<bool>>(ev => Task.FromResult(predicate(ev)));

            return builder.Handle(null, taskPredicate, typeof(THandlerType), configure);
        }

        public static IntegrationFlowBuilder Handle(this IntegrationFlowBuilder builder, Action<CloudEvent> handler, Predicate<CloudEvent> predicate = null)
        {
            var func = new Func<CloudEvent, Task>(ev =>
            {
                handler(ev);

                return Task.CompletedTask;
            });

            return builder.Handle(func, predicate);
        }

        public static IntegrationFlowBuilder Handle(this IntegrationFlowBuilder builder, Func<CloudEvent, Task> handler, Predicate<CloudEvent> predicate = null)
        {
            if (predicate == null)
            {
                predicate = ev => true;
            }
            
            var taskPredicate = new Func<CloudEvent, Task<bool>>(ev => Task.FromResult(predicate(ev)));

            return builder.Handle(handler, taskPredicate);
        }

        public static IntegrationFlowBuilder Handle(this IntegrationFlowBuilder builder, Func<CloudEvent, Task> handler,
            Func<CloudEvent, Task<bool>> predicate = null, Type handlerType = null, MulticastDelegate configureHandler = null)
        {
            Task<CloudEventsComponent> Handler(IServiceProvider provider)
            {
                var eventLinkInitializer = provider.GetRequiredService<EventLinkInitializer>();
                var typeToEventLinksConverter = provider.GetRequiredService<ITypeToEventLinksConverter>();

                if (predicate == null)
                {
                    predicate = cloudEvent => Task.FromResult(true);
                }

                predicate = predicate + (ev =>
                {
                    var attrs = ev.GetAttributes();

                    if (attrs.ContainsKey(EventFrameworkIntegrationFlowEventExtension.EventFrameworkIntegrationFlowAttributeName) == false)
                    {
                        return Task.FromResult(false);
                    }

                    var flowId = attrs[EventFrameworkIntegrationFlowEventExtension.EventFrameworkIntegrationFlowAttributeName] as string;

                    return Task.FromResult(string.Equals(builder.Id, flowId));
                });

                if (handlerType != null)
                {
                    var links = typeToEventLinksConverter.Create(provider, handlerType, predicate, configureHandler);

                    foreach (var eventLink in links)
                    {
                        eventLinkInitializer.Initialize(eventLink);
                    }
                }

                if (handler != null)
                {
                    var link = new EventLink(predicate, handler);
                    eventLinkInitializer.Initialize(link);
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
