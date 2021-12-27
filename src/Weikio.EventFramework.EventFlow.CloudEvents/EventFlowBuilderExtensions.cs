using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventAggregator.Core;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public static class EventFlowBuilderExtensions
    {
        public static EventFlowBuilder Channel(this EventFlowBuilder builder, string channelName, Predicate<CloudEvent> predicate = null)
        {
            var component = new ChannelComponentBuilder(channelName, predicate);
            
            builder.Component(component.Build);

            return builder;
        }

        public static EventFlowBuilder Transform(this EventFlowBuilder builder, Func<CloudEvent, CloudEvent> transform, Predicate<CloudEvent> predicate = null)
        {
            var component = new CloudEventsComponent(transform, predicate);
            builder.Component(component);

            return builder;
        }

        public static EventFlowBuilder Filter(this EventFlowBuilder builder, Func<CloudEvent, Filter> filter)
        {
            var componentBuilder = new FilterComponentBuilder(filter);
            builder.Component(componentBuilder.Build);

            return builder;
        }

        public static EventFlowBuilder Filter(this EventFlowBuilder builder, Predicate<CloudEvent> filter)
        {
            var componentBuilder = new FilterComponentBuilder(filter);
            builder.Component(componentBuilder.Build);

            return builder;
        }

        public static EventFlowBuilder Branch(this EventFlowBuilder builder,
            params (Predicate<CloudEvent> Predicate, Action<EventFlowBuilder> BuildBranch)[] branches)
        {
            async Task<CloudEventsComponent> Handler(ComponentFactoryContext context)
            {
                var instanceManager = context.ServiceProvider.GetRequiredService<ICloudEventFlowManager>();
                var channelManager = context.ServiceProvider.GetRequiredService<IChannelManager>();

                var createdFlows = new List<(Predicate<CloudEvent> Predicate, string ChannelId)>();

                for (var index = 0; index < branches.Length; index++)
                {
                    var branchChannelName = $"system/flows/branches/{Guid.NewGuid()}/in";
                    var branchChannelOptions = new CloudEventsChannelOptions() { Name = branchChannelName };
                    var branchInputChannel = new CloudEventsChannel(branchChannelOptions);
                    channelManager.Add(branchInputChannel);

                    var branch = branches[index];
                    var flowBuilder = EventFlowBuilder.From(branchChannelName);
                    branch.BuildBranch(flowBuilder);

                    var branchFlow = await flowBuilder.Build(context.ServiceProvider);

                    await instanceManager.Execute(branchFlow,
                        new EventFlowInstanceOptions() { Id = $"system/flows/branches/{Guid.NewGuid()}" });

                    createdFlows.Add((branch.Predicate, branchChannelName));
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

            builder.Component(Handler);

            return builder;
        }

        public static EventFlowBuilder Handle<THandlerType>(this EventFlowBuilder builder, Predicate<CloudEvent> predicate = null,
            Action<THandlerType> configure = null)
        {
            if (predicate == null)
            {
                predicate = ev => true;
            }

            var taskPredicate = new Func<CloudEvent, Task<bool>>(ev => Task.FromResult(predicate(ev)));

            return builder.Handle(null, taskPredicate, typeof(THandlerType), configure);
        }

        public static EventFlowBuilder Handle(this EventFlowBuilder builder, Action<CloudEvent> handler, Predicate<CloudEvent> predicate = null)
        {
            var func = new Func<CloudEvent, Task>(ev =>
            {
                handler(ev);

                return Task.CompletedTask;
            });

            return builder.Handle(func, predicate);
        }

        public static EventFlowBuilder Handle(this EventFlowBuilder builder, Func<CloudEvent, Task> handler, Predicate<CloudEvent> predicate = null)
        {
            if (predicate == null)
            {
                predicate = ev => true;
            }

            var taskPredicate = new Func<CloudEvent, Task<bool>>(ev => Task.FromResult(predicate(ev)));

            return builder.Handle(handler, taskPredicate);
        }

        public static EventFlowBuilder Handle(this EventFlowBuilder builder, Func<CloudEvent, Task> handler,
            Func<CloudEvent, Task<bool>> predicate = null, Type handlerType = null, MulticastDelegate configureHandler = null)
        {
            Task<CloudEventsComponent> Handler(ComponentFactoryContext context)
            {
                var eventLinkInitializer = context.ServiceProvider.GetRequiredService<EventLinkInitializer>();
                var typeToEventLinksConverter = context.ServiceProvider.GetRequiredService<ITypeToEventLinksConverter>();

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

                    return Task.FromResult(string.Equals(context.ComponentChannelName, channelId));
                });

                if (handlerType != null)
                {
                    var links = typeToEventLinksConverter.Create(context.ServiceProvider, handlerType, predicate, configureHandler);

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
                    var aggr = context.ServiceProvider.GetRequiredService<ICloudEventAggregator>();
                    await aggr.Publish(ev);

                    return ev;
                });

                return Task.FromResult(aggregatorComponent);
            }

            builder.Component(Handler);

            return builder;
        }
    }
}
