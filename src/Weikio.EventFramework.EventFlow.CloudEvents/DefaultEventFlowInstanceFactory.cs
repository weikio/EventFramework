using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class DefaultEventFlowInstanceFactory : IEventFlowInstanceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DefaultEventFlowInstanceFactory> _logger;
        private readonly IOptions<EventFlowChannelDefaultComponents> _options;
        private readonly ICloudEventsChannelManager _channelManager;

        public DefaultEventFlowInstanceFactory(IServiceProvider serviceProvider,
            ILogger<DefaultEventFlowInstanceFactory> logger,
            IOptions<EventFlowChannelDefaultComponents> options, ICloudEventsChannelManager channelManager)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options;
            _channelManager = channelManager;
        }

        public async Task<EventFlowInstance> Create(EventFlow eventFlow, EventFlowInstanceOptions options)
        {
            // Component channel names are created based on the index, so 0, 1, 2 etc.
            // Is there any use for the fact that we use index numbers? Maybe just give Guid to the component and use that as channel name.
            // Or, why we create the components at this point? Why doesn't the DefaultCloudEventsIntegrationFlowManager create them?
            // Or maybe the channels should be created here

            var defaultComponents = _options.Value.ComponentsFactory(eventFlow, options);
            var createdComponentChannels = new List<string>();

            // We want every flow to have a output channel where every event ends up
            var outputChannelOptions = new CloudEventsChannelOptions() { Name = options.OutputChannel };

            outputChannelOptions.Endpoints.Add(new CloudEventsEndpoint(async ev =>
            {
                // Insert an endpoint which transfer the event to the desired channel if event has attribute eventFramework_eventFlow_endpoint
                var attrs = ev.GetAttributes();

                if (attrs.ContainsKey(EventFrameworkEventFlowEndpointEventExtension.EventFrameworkEventFlowEndpointAttributeName) == false)
                {
                    return;
                }

                var targetChannel = attrs[EventFrameworkEventFlowEndpointEventExtension.EventFrameworkEventFlowEndpointAttributeName] as string;

                if (string.IsNullOrWhiteSpace(targetChannel))
                {
                    return;
                }

                var channel = _serviceProvider.GetRequiredService<IChannelManager>().Get(targetChannel);

                // After a transfer, we want to remove the attribute so that the event doesn't get stuck in a loop
                attrs.Remove(EventFrameworkEventFlowEndpointEventExtension.EventFrameworkEventFlowEndpointAttributeName);

                await channel.Send(ev);
            }));

            var endpointChannel = new CloudEventsChannel(outputChannelOptions);

            _channelManager.Add(endpointChannel);

            // Create the endpoint which transfers all the processed events into the flow's output channel.
            // We can use the output channel to create subscriptions between flows etc.
            var outputChannelEndpoint = new CloudEventsEndpoint(async ev =>
            {
                var flowOutputChannel = _channelManager.Get(options.OutputChannel);
                await flowOutputChannel.Send(ev);
            });

            eventFlow.Endpoints.Add(outputChannelEndpoint);

            // We don't actually create a single channel for the flow but a single channel for each component in the flow
            // Interceptors are added to each channel
            // Endpoints are added only to the last channel
            // Each component channel has an endpoint which points to the next component channel 

            var steps = new List<Step>();

            for (var index = 0; index < eventFlow.ComponentFactories.Count; index++)
            {
                var componentFactory = eventFlow.ComponentFactories[index];

                // TODO: Find a place for this
                var componentChannelName = $"system/flows/{options.Id}/channels/{index}";
                var isLastComponent = eventFlow.ComponentFactories.Count <= index + 1;

                string nextComponentChannelName = null;

                Step step;

                if (!isLastComponent)
                {
                    // TODO: Find a place for this
                    nextComponentChannelName = $"system/flows/{options.Id}/channels/{index + 1}";
                    step = new Step(componentChannelName, nextComponentChannelName);
                }
                else
                {
                    step = new Step(componentChannelName, outputChannelOptions.Name);
                }
                
                var tags = new List<(string, object)> { ("flowid", options.Id), ("nextchannelname", nextComponentChannelName), ("step", step), ("steps", steps) };

                var context = new ComponentFactoryContext(_serviceProvider,
                    index,
                    componentChannelName, tags);

                var component = await componentFactory(context);
                eventFlow.Components.Add(component);

                var componentChannelOptions = new CloudEventsChannelOptions() { Name = componentChannelName };

                // Insert an interceptor which adds a IntegrationFlowExtension to the event's attributes
                // The idea is that when event moves around flows and sub flows, it always knows the context
                componentChannelOptions.Interceptors.Add((InterceptorTypeEnum.PreComponents, new CurrentFlowChannelInterceptor(componentChannelName)));

                // If we have some default components configured, add them to each flow channel
                if (defaultComponents?.Any() == true)
                {
                    foreach (var defaultComponent in defaultComponents)
                    {
                        componentChannelOptions.Components.Add(defaultComponent);
                    }
                }

                componentChannelOptions.Components.Add(component);
                componentChannelOptions.Interceptors.AddRange(eventFlow.Interceptors);

                if (isLastComponent)
                {
                    componentChannelOptions.Endpoints.AddRange(eventFlow.Endpoints);
                }
                else
                {
                    var nextChannelEndpoint = new CloudEventsEndpoint(async ev =>
                    {
                        var nextComponentChannel = _channelManager.Get(nextComponentChannelName);
                        await nextComponentChannel.Send(ev);
                    });

                    componentChannelOptions.Endpoints.Add(nextChannelEndpoint);
                }

                var componentChannel = new CloudEventsChannel(componentChannelOptions);

                _channelManager.Add(componentChannel);

                createdComponentChannels.Add(componentChannel.Name);
                steps.Add(step);
            }

            var flowInstance = new EventFlowInstance(eventFlow, options);

            _logger.LogDebug(
                "Creating a new channel for integration flow with Id {FlowId}", flowInstance.Id);

            // We always want to create a channel for our flow instance.
            // If the flow is based on event source, we deliver events from the source to this new flow channel.
            // If the flow is based on another channel, we subscribe source channel to the flow channel.
            // If the flow doesn't have source, we just create the flow channel and wait for something to get delivered to it. 
            var flowInputChannelName = flowInstance.InputChannel;
            var flowInputChannelOptions = new CloudEventsChannelOptions() { Name = flowInputChannelName };

            if (defaultComponents?.Any() == true)
            {
                foreach (var defaultComponent in defaultComponents)
                {
                    flowInputChannelOptions.Components.Add(defaultComponent);
                }
            }

            var stepOutputs = new List<string>();

            if (createdComponentChannels.Any())
            {
                var firstComponentChannelId = createdComponentChannels.First();

                var firstComponentEndpoint = new CloudEventsEndpoint(async ev =>
                {
                    var firstComponentChannel = _channelManager.Get(firstComponentChannelId);
                    await firstComponentChannel.Send(ev);
                });

                flowInputChannelOptions.Endpoints.Add(firstComponentEndpoint);
                stepOutputs.Add(firstComponentChannelId);
            }
            else
            {
                flowInputChannelOptions.Endpoints.AddRange(flowInstance.Endpoints);

                foreach (var _ in flowInstance.Endpoints)
                {
                    stepOutputs.Add("endpoint");
                }
            }

            var flowInputChannel = new CloudEventsChannel(flowInputChannelOptions);
            _channelManager.Add(flowInputChannel);
            
            steps.Insert(0, new Step(flowInputChannelName, stepOutputs));

            _logger.LogInformation("Created new input channel for flow {FlowId} with name {ChannelId}", flowInstance.Id, flowInputChannelName);

            flowInstance.Steps = steps;
            
            return flowInstance;
        }
    }
}
