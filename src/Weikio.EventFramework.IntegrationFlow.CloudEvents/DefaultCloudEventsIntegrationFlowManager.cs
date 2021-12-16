using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Components;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    internal class DefaultCloudEventsIntegrationFlowManager : List<IntegrationFlowInstance>, ICloudEventsIntegrationFlowManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventSourceInstanceManager _eventSourceInstanceManager;
        private readonly IEventSourceProvider _eventSourceProvider;
        private readonly ILogger<DefaultCloudEventsIntegrationFlowManager> _logger;
        private readonly ICloudEventsChannelManager _channelManager;
        private readonly IntegrationFlowProvider _integrationFlowProvider;
        private readonly IIntegrationFlowInstanceFactory _instanceFactory;
        private readonly IOptions<IntegrationFlowChannelDefaultComponents> _options;

        public DefaultCloudEventsIntegrationFlowManager(IServiceProvider serviceProvider, IEventSourceInstanceManager eventSourceInstanceManager,
            IEventSourceProvider eventSourceProvider, ILogger<DefaultCloudEventsIntegrationFlowManager> logger,
            ICloudEventsChannelManager channelManager, IntegrationFlowProvider integrationFlowProvider, IIntegrationFlowInstanceFactory instanceFactory,
            IOptions<IntegrationFlowChannelDefaultComponents> options)
        {
            _serviceProvider = serviceProvider;
            _eventSourceInstanceManager = eventSourceInstanceManager;
            _eventSourceProvider = eventSourceProvider;
            _logger = logger;
            _channelManager = channelManager;
            _integrationFlowProvider = integrationFlowProvider;
            _instanceFactory = instanceFactory;
            _options = options;
        }

        public async Task Execute(IntegrationFlow integrationFlow)
        {
            var options = new IntegrationFlowInstanceOptions() { Id = "flowinstance_" + Guid.NewGuid() };

            await Execute(integrationFlow, options);
        }
        
        public async Task Execute(IntegrationFlow integrationFlow, IntegrationFlowInstanceOptions instanceOptions)
        {
            var instance = await _instanceFactory.Create(integrationFlow, instanceOptions);

            await Execute(instance);
        }
        
        public async Task Execute(IntegrationFlowInstance flowInstance)
        {
            _logger.LogInformation("Executing integration flow with ID {Id}", flowInstance.Id);

            // We always want to create a channel for our flow instance.
            // If the flow is based on event source, we deliver events from the source to this new flow channel.
            // If the flow is based on another channel, we subscribe source channel to the flow channel.
            // If the flow doesn't have source, we just create the flow channel and wait for something to get delivered to it. 
            _logger.LogDebug(
                "Creating a new channel for integration flow with Id {FlowId}", flowInstance.Id);

            // We don't actually create a single channel for the flow but a single channel for each component in the flow
            // Interceptors are added to each channel
            // Endpoints are added only to the last channel
            // Each component channel has an endpoint which points to the next component channel 
            var channelName = flowInstance.InputChannel;
            var flowChannelOptions = new CloudEventsChannelOptions() { Name = channelName };

            var defaultComponents = _options.Value.ComponentsFactory(flowInstance.IntegrationFlow, flowInstance.FlowInstanceOptions);

            if (defaultComponents?.Any() == true)
            {
                foreach (var defaultComponent in defaultComponents)
                {
                    flowChannelOptions.Components.Add(defaultComponent);
                }
            }

            var createdComponentChannels = new List<string>();

            for (var index = 0; index < flowInstance.Components.Count; index++)
            {
                var component = flowInstance.Components[index];

                // TODO: Find a place for this
                var componentChannelName = $"system/flows/{flowInstance.Id}/componentchannels/{index}";
                var componentChannelOptions = new CloudEventsChannelOptions() { Name = componentChannelName };

                // Insert a component which adds a IntegrationFlowExtension to the event's attributes
                // The idea is that when events moves around flows and sub flows, it always knows the context
                if (defaultComponents?.Any() == true)
                {
                    foreach (var defaultComponent in defaultComponents)
                    {
                        componentChannelOptions.Components.Add(defaultComponent);
                    }
                }

                componentChannelOptions.Components.Add(component);
                componentChannelOptions.Interceptors.AddRange(flowInstance.Interceptors);

                var isLastComponent = flowInstance.Components.Count <= index + 1;

                if (isLastComponent)
                {
                    componentChannelOptions.Endpoints.AddRange(flowInstance.Endpoints);
                }
                else
                {
                    // TODO: Find a place for this
                    var nextComponentChannelName = $"system/flows/{flowInstance.Id}/componentchannels/{index + 1}";

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
            }

            if (createdComponentChannels.Any())
            {
                var firstComponentChannelId = createdComponentChannels.First();

                var firstComponentEndpoint = new CloudEventsEndpoint(async ev =>
                {
                    var nextComponentChannel = _channelManager.Get(firstComponentChannelId);
                    await nextComponentChannel.Send(ev);
                });

                flowChannelOptions.Endpoints.Add(firstComponentEndpoint);
            }
            else
            {
                flowChannelOptions.Endpoints.AddRange(flowInstance.Endpoints);
            }

            var flowChannel = new CloudEventsChannel(flowChannelOptions);

            _channelManager.Add(flowChannel);

            _logger.LogInformation("Created new input channel for flow {FlowId} with name {ChannelId}", flowInstance.Id, channelName);

            // Create event source instance and a channel based on the input
            // But only create event source instance if needed. If source is defined, try to find existing event source instance or channel
            if (string.IsNullOrWhiteSpace(flowInstance.Source) && flowInstance.EventSourceType != null)
            {
                _logger.LogDebug("Integration flow with ID {Id} requires a new event source", flowInstance.Id);

                var esOptions = new EventSourceInstanceOptions();

                if (flowInstance.ConfigureEventSourceInstanceOptions != null)
                {
                    flowInstance.ConfigureEventSourceInstanceOptions(esOptions);
                }
                else
                {
                    esOptions.Autostart = true;
                    esOptions.PollingFrequency = TimeSpan.FromSeconds(2);
                }

                esOptions.Id = flowInstance.Id;

                TypePluginCatalog typePluginCatalog = null;

                if (flowInstance.EventSourceType != null && esOptions.EventSourceDefinition == null)
                {
                    typePluginCatalog = new TypePluginCatalog(flowInstance.EventSourceType);
                    await typePluginCatalog.Initialize();

                    var definition = typePluginCatalog.Single();
                    esOptions.EventSourceDefinition = definition.Name;
                }

                esOptions.PublishToChannel = true;

                // Make sure we have registered the event source defined in the integration flow
                var isDefinitionKnown = true;

                try
                {
                    _eventSourceProvider.Get(esOptions.EventSourceDefinition);
                }
                catch (UnknownEventSourceException)
                {
                    _logger.LogDebug("Event source definition {Definition} does not exist, registering it automatically", esOptions.EventSourceDefinition);
                    isDefinitionKnown = false;
                }

                if (isDefinitionKnown == false)
                {
                    // Register if needed
                    var catalog = new PluginFrameworkEventSourceCatalog(typePluginCatalog);
                    await _eventSourceProvider.AddCatalog(catalog);

                    _logger.LogDebug("Event source definition {Definition} registered", esOptions.EventSourceDefinition);
                }

                esOptions.TargetChannelName = flowInstance.InputChannel;

                await _eventSourceInstanceManager.Create(esOptions);

                _logger.LogDebug("New Event source with Id {EsId} created for Integration flow with Id {Id}", esOptions.Id, flowInstance.Id);

                _logger.LogInformation("Executed flow with id {FlowId}. Source: New Event Source with id {EsId}", flowInstance.Id, esOptions.Id);

                Add(flowInstance);

                return;
            }

            _logger.LogDebug(
                "Source {Source} is defined for integration flow with ID {Id}. Trying to find existing channel or event source instance based on source",
                flowInstance.Source, flowInstance.Id);
            var existingEsInstance = _eventSourceInstanceManager.Get(flowInstance.Source);

            CloudEventsChannel sourceChannel = null;

            if (existingEsInstance == null && !string.IsNullOrWhiteSpace(flowInstance.Source))
            {
                _logger.LogDebug("No existing Event Source Instance found with Id {Source}. Trying to find an existing channel", flowInstance.Source);

                try
                {
                    sourceChannel = _channelManager.Get(flowInstance.Source);

                    // Channel manager (for some reason) returns a channel if there is only one channel even if the names do not match
                    if (!string.Equals(sourceChannel.Name, flowInstance.Source))
                    {
                        throw new UnknownChannelException(sourceChannel.Name);
                    }
                }
                catch (Exception ex) when (ex is UnknownChannelException || ex is NoChannelsConfiguredException)
                {
                    _logger.LogDebug("Could not locate channel using name {Source} for Flow with Id {FlowId}", flowInstance.Source, flowInstance.Id);

                    throw new UnknownIntegrationFlowSourceException();
                }

                _logger.LogDebug("Found existing channel {ChannelName} as a source for integration flow with Id {Source}", flowInstance.Source,
                    flowInstance.Source);
            }

            if (sourceChannel == null && existingEsInstance != null)
            {
                _logger.LogDebug("Using event source instances {EsInstanceId} internal channel as the source channel for flow {FlowId}",
                    existingEsInstance.InternalChannelId, flowInstance.Id);

                var esInstanceChannelName = existingEsInstance.InternalChannelId;
                sourceChannel = _channelManager.Get(esInstanceChannelName);
            }

            if (sourceChannel != null)
            {
                if (sourceChannel.IsPubSub == false)
                {
                    // TODO: We could actually add a new endpoint to the source channel to get around this issue
                    throw new NotSupportedChannelTypeForIntegrationFlow();
                }

                sourceChannel.Subscribe(flowChannel);
            }
            else
            {
                _logger.LogDebug("Flow {FlowId} does not have source. To run the flow, event must be delivered to its input channel {ChannelId}",
                    flowInstance.Id, flowInstance.InputChannel);
            }

            if (existingEsInstance != null)
            {
                _logger.LogInformation("Executed flow with id {FlowId}. Source: Existing Event Source with id {EsId}", flowInstance.Id, existingEsInstance.Id);
            }
            else if (sourceChannel != null)
            {
                _logger.LogInformation("Executed flow with id {FlowId}. Source: Channel with name {SourceChannel}", flowInstance.Id, sourceChannel.Name);
            }
            else
            {
                _logger.LogInformation("Executed flow with id {FlowId}. Source: None", flowInstance.Id);
            }

            Add(flowInstance);
        }

        public async Task<IntegrationFlowInstance> Create(IntegrationFlowDefinition flowDefinition, string id = null, string description = null,
            object configuration = null)
        {
            _logger.LogInformation("Creating Integration Flow using registered flow with definition {Definition}", flowDefinition);

            var flowType = _integrationFlowProvider.Get(flowDefinition);

            var flowInstance = await flowType.Create(_serviceProvider);

            return flowInstance;

            // CloudEventsIntegrationFlowBase flow;
            //
            // if (configuration != null)
            // {
            //     flow = (CloudEventsIntegrationFlowBase)ActivatorUtilities.CreateInstance(_serviceProvider, flowType, new object[] { configuration });
            // }
            // else
            // {
            //     flow = (CloudEventsIntegrationFlowBase)ActivatorUtilities.CreateInstance(_serviceProvider, flowType);
            // }
            //
            // var result = await flow.Flow.Build(_serviceProvider);
            //
            // return result;
        }

        public List<IntegrationFlowInstance> List()
        {
            return this;
        }

        public IntegrationFlowInstance Get(string id)
        {
            var result = this.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.InvariantCultureIgnoreCase));

            if (result != null)
            {
                return result;
            }

            throw new UnknownIntegrationFlowInstance(id,
                $"Could not find Integration Flow Instance with ID: {id}.{Environment.NewLine}{Environment.NewLine}Available flows:{Environment.NewLine}{string.Join(",", this.Select(x => x.Id))}");
        }
    }

    public class UnknownIntegrationFlowInstance : Exception
    {
        public string Id { get; }

        public UnknownIntegrationFlowInstance(string id, string message) : base(message)
        {
            Id = id;
        }
    }
}
