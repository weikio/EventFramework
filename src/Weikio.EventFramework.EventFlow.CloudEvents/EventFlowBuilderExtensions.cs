using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Components;
using Weikio.EventFramework.EventAggregator.Core;
using Weikio.EventFramework.EventFlow.Abstractions;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public static class EventFlowBuilderExtensions
    {
        public static EventFlowBuilder Channel(this EventFlowBuilder builder, string channelName, Predicate<CloudEvent> predicate = null)
        {
            Task<CloudEventsComponent> Handler(ComponentFactoryContext context)
            {
                var channelManager = context.ServiceProvider.GetRequiredService<ICloudEventsChannelManager>();

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

        public static EventFlowBuilder Transform(this EventFlowBuilder builder, Func<CloudEvent, CloudEvent> transform)
        {
            var component = new CloudEventsComponent(transform);
            builder.Register(component);

            return builder;
        }

        public static EventFlowBuilder Filter(this EventFlowBuilder builder, Func<CloudEvent, Filter> filter)
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
        
        public static EventFlowBuilder Filter(this EventFlowBuilder builder, Predicate<CloudEvent> filter)
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
        
        public static EventFlowBuilder Flow(this EventFlowBuilder builder, Action<EventFlowBuilder> buildFlow, 
            Predicate<CloudEvent> predicate = null)
        {
            async Task<CloudEventsComponent> Handler(ComponentFactoryContext context)
            {
                var instanceManager = context.ServiceProvider.GetRequiredService<ICloudEventFlowManager>();
                var channelManager = context.ServiceProvider.GetRequiredService<IChannelManager>();

                var flowBuilder = EventFlowBuilder.From();
                buildFlow(flowBuilder);

                var subflowId = $"system/flows/{context.Options.Id}/subflows/{context.CurrentComponentIndex}";

                var subflow = await flowBuilder.Build(context.ServiceProvider);
                    
                await instanceManager.Execute(subflow, new EventFlowInstanceOptions()
                {
                    Id = subflowId
                });

                var flowComponent = new CloudEventsComponent(async ev =>
                {
                    if (!string.IsNullOrWhiteSpace(context.NextComponentChannelName))
                    {
                        var ext = new EventFrameworkEventFlowEndpointEventExtension(context.NextComponentChannelName);
                        ext.Attach(ev);
                    }

                    var targetFlow = instanceManager.Get(subflowId);
                    var targetFlowInputChannel = targetFlow.InputChannel;

                    var targetChannel = channelManager.Get(targetFlowInputChannel);
                    await targetChannel.Send(ev);

                    return null;
                }, predicate);

                return flowComponent;
            }

            builder.Register(Handler);

            return builder;
        }
        
        public static EventFlowBuilder Flow(this EventFlowBuilder builder, EventFlowDefinition flowDefinition, 
            Predicate<CloudEvent> predicate = null, string flowId = null)
        {
            Task<CloudEventsComponent> Handler(ComponentFactoryContext context)
            {
                var instanceManager = context.ServiceProvider.GetRequiredService<ICloudEventFlowManager>();
                var channelManager = context.ServiceProvider.GetRequiredService<IChannelManager>();
                
                var flowComponent = new CloudEventsComponent(async ev =>
                {
                    if (!string.IsNullOrWhiteSpace(context.NextComponentChannelName))
                    {
                        var ext = new EventFrameworkEventFlowEndpointEventExtension(context.NextComponentChannelName);
                        ext.Attach(ev);
                    }

                    EventFlowInstance targetFlow;
                    if (!string.IsNullOrWhiteSpace(flowId))
                    {
                        targetFlow = instanceManager.Get(flowId);
                    }
                    else
                    {
                        var allFlows = instanceManager.List();
                        targetFlow = allFlows.FirstOrDefault(x => Equals(x.FlowDefinition, flowDefinition));
                    }

                    if (targetFlow == null)
                    {
                        throw new UnknownEventFlowInstance("", "Couldn't locate target flow using id or flow definition");
                    }
                    
                    var targetFlowInputChannel = targetFlow.InputChannel;

                    var targetChannel = channelManager.Get(targetFlowInputChannel);
                    await targetChannel.Send(ev);

                    return null;
                }, predicate);

                return Task.FromResult(flowComponent);
            }

            builder.Register(Handler);

            return builder;
        }
        
        public static EventFlowBuilder Branch(this EventFlowBuilder builder, params (Predicate<CloudEvent> Predicate, Action<EventFlowBuilder> BuildBranch)[] branches )
        {
            async Task<CloudEventsComponent> Handler(ComponentFactoryContext context)
            {
                var instanceManager = context.ServiceProvider.GetRequiredService<ICloudEventFlowManager>();
                var channelManager = context.ServiceProvider.GetRequiredService<IChannelManager>();

                var createdFlows = new List<(Predicate<CloudEvent> Predicate, string ChannelId)>();

                for (var index = 0; index < branches.Length; index++)
                {
                    var branchChannelName = $"system/flows/{context.Options.Id}/branches/input_{context.CurrentComponentIndex}/{index}";
                    var branchChannelOptions = new CloudEventsChannelOptions() { Name = branchChannelName };
                    var branchInputChannel = new CloudEventsChannel(branchChannelOptions);
                    channelManager.Add(branchInputChannel);
                    
                    var branch = branches[index];
                    var flowBuilder = EventFlowBuilder.From(branchChannelName);
                    branch.BuildBranch(flowBuilder);

                    var branchFlow = await flowBuilder.Build(context.ServiceProvider);
                    
                    await instanceManager.Execute(branchFlow, new EventFlowInstanceOptions()
                    {
                        Id = $"{context.Options.Id}/branches/{context.CurrentComponentIndex}/{index}"
                    });

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

            builder.Register(Handler);

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

                    var flowId = attrs[EventFrameworkEventFlowEventExtension.EventFrameworkEventFlowAttributeName] as string;

                    return Task.FromResult(string.Equals(context.Options.Id, flowId));
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

            builder.Register(Handler);

            return builder;
        }
    }
}
