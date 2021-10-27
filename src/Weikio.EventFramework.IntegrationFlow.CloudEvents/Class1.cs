using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventAggregator.Core;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class CloudEventsIntegrationFlow : IntegrationFlowBase<CloudEvent>
    {
    }

    public class IntegrationFlowBuilder : IBuilder<CloudEventsIntegrationFlow>
    {
        private ArrayList _flow = new ArrayList();
        private List<Func<IServiceProvider, Task<CloudEventsComponent>>> _components = new List<Func<IServiceProvider, Task<CloudEventsComponent>>>();
        private Action<EventSourceInstanceOptions> _configureEventSourceInstance;
        private Type _eventSourceType;
        public string Id { get; private set; } = "test";

        public static IntegrationFlowBuilder From()
        {
            var builder = new IntegrationFlowBuilder();

            return builder;
        }

        public static IntegrationFlowBuilder From<TEventSourceType>(Action<EventSourceInstanceOptions> configureInstance = null)
        {
            var builder = new IntegrationFlowBuilder { _configureEventSourceInstance = configureInstance, _eventSourceType = typeof(TEventSourceType)};

            return builder;
        }

        public async Task<CloudEventsIntegrationFlow> Build(IServiceProvider serviceProvider)
        {
            var result = new CloudEventsIntegrationFlow
            {
                Id = Id, Description = "description", 
                ConfigureEventSourceInstanceOptions = _configureEventSourceInstance
            };

            foreach (var componentBuilder in _components)
            {
                var component = await componentBuilder(serviceProvider);
                result.Components.Add(component);
            }

            return result;
        }

        // public IntegrationFlowBuilder Register(object flowItem)
        // {
        //     _flow.Add(flowItem);
        //
        //     return this;
        // }
        //

        public IntegrationFlowBuilder Register(CloudEventsComponent component)
        {
            _components.Add(provider => Task.FromResult(component));

            return this;
        }

        public IntegrationFlowBuilder Register(Func<IServiceProvider, Task<CloudEventsComponent>> componentBuilder)
        {
            _components.Add(componentBuilder);

            return this;
        }
    }
    
    public class CloudEventsIntegrationFlowManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventSourceInstanceManager _eventSourceInstanceManager;

        public CloudEventsIntegrationFlowManager(IServiceProvider serviceProvider, IEventSourceInstanceManager eventSourceInstanceManager)
        {
            _serviceProvider = serviceProvider;
            _eventSourceInstanceManager = eventSourceInstanceManager;
        }

        Task CreateResources()
        {
            // Create event source if needed
            // Create channels if needed
            // Create channel subscriptions

            return Task.CompletedTask;
        }

        Task Start()
        {
            return Task.CompletedTask;
        }

        public async Task Execute(CloudEventsIntegrationFlow flow)
        {
            // Create event source instance and a channel based on the input
            var esOptions = new EventSourceInstanceOptions();
            flow.ConfigureEventSourceInstanceOptions(esOptions);

            esOptions.ConfigureChannel = options =>
            {
                options.Components.AddRange(flow.Components);
            };

            var es = await _eventSourceInstanceManager.Create(esOptions);
            
            await _eventSourceInstanceManager.Start(es);
        }
    }

    public static class EventFrameworkBuilderExtensions
    {
        public static IEventFrameworkBuilder AddCloudEventIntegrationFlows(this IEventFrameworkBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<CloudEventsIntegrationFlowManager>();
            
            return builder;
        }
    }

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

                    if (attrs.ContainsKey("flow_id") == false)
                    {
                        return false;
                    }

                    var flowId = attrs["flow_id"] as string;

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

    public class ChannelComponent : CloudEventsComponent
    {
        public ChannelComponent(IChannel channel, Predicate<CloudEvent> predicate)
        {
            Func = async ev =>
            {
                await channel.Send(ev);

                return ev;
            };

            Predicate = predicate;
        }
    }
}
