using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    internal class DefaultCloudEventsIntegrationFlowManager : List<CloudEventsIntegrationFlow>, ICloudEventsIntegrationFlowManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventSourceInstanceManager _eventSourceInstanceManager;
        private readonly IEventSourceProvider _eventSourceProvider;
        private readonly ILogger<DefaultCloudEventsIntegrationFlowManager> _logger;
        private readonly ICloudEventsChannelManager _channelManager;
        private readonly IntegrationFlowProvider _integrationFlowProvider;

        public DefaultCloudEventsIntegrationFlowManager(IServiceProvider serviceProvider, IEventSourceInstanceManager eventSourceInstanceManager,
            IEventSourceProvider eventSourceProvider, ILogger<DefaultCloudEventsIntegrationFlowManager> logger, ICloudEventsChannelManager channelManager, IntegrationFlowProvider integrationFlowProvider)
        {
            _serviceProvider = serviceProvider;
            _eventSourceInstanceManager = eventSourceInstanceManager;
            _eventSourceProvider = eventSourceProvider;
            _logger = logger;
            _channelManager = channelManager;
            _integrationFlowProvider = integrationFlowProvider;
        }

        public async Task Execute(CloudEventsIntegrationFlow flow)
        {
            _logger.LogInformation("Executing integration flow with ID {Id}", flow.Id);

            // Create event source instance and a channel based on the input
            // But only create event source instance if needed. If source is defined, try to find existing event source instance or channel
            if (string.IsNullOrWhiteSpace(flow.Source))
            {
                _logger.LogDebug("Integration flow with ID {Id} requires a new event source", flow.Id);

                var esOptions = new EventSourceInstanceOptions();

                if (flow.ConfigureEventSourceInstanceOptions != null)
                {
                    flow.ConfigureEventSourceInstanceOptions(esOptions);
                }
                else
                {
                    esOptions.Autostart = true;
                    esOptions.PollingFrequency = TimeSpan.FromSeconds(2);
                }

                esOptions.Id = flow.Id;

                TypePluginCatalog typePluginCatalog = null;

                if (flow.EventSourceType != null && esOptions.EventSourceDefinition == null)
                {
                    typePluginCatalog = new TypePluginCatalog(flow.EventSourceType);
                    await typePluginCatalog.Initialize();

                    var definition = typePluginCatalog.Single();
                    esOptions.EventSourceDefinition = definition.Name;
                }

                esOptions.PublishToChannel = false;

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

                esOptions.ConfigureChannel = options =>
                {
                    options.Components.AddRange(flow.Components);
                    options.Interceptors.AddRange(flow.Interceptors);
                };

                await _eventSourceInstanceManager.Create(esOptions);

                _logger.LogDebug("New Event source with Id {EsId} created for Integration flow with Id {Id}", esOptions.Id, flow.Id);

                Add(flow);
                
                return;
            }

            _logger.LogDebug(
                "Source {Source} is defined for integration flow with ID {Id}. Trying to find existing channel or event source instance based on source",
                flow.Source, flow.Id);
            var existingEsInstance = _eventSourceInstanceManager.Get(flow.Source);

            CloudEventsChannel sourceChannel = null;

            if (existingEsInstance == null)
            {
                _logger.LogDebug("No existing Event Source Instance found with Id {Source}. Trying to find an existing channel", flow.Source);

                try
                {
                    sourceChannel = _channelManager.Get(flow.Source);
                }
                catch (Exception ex) when (ex is UnknownChannelException || ex is NoChannelsConfiguredException)
                {
                    _logger.LogDebug("Could not locate channel using name {Source} for Flow with Id {FlowId}", flow.Source, flow.Id);

                    throw new UnknownIntegrationFlowSourceException();
                }
                
                _logger.LogDebug("Found existing channel {ChannelName} as a source for integration flow with Id {Source}", flow.Source, flow.Source);
            }

            _logger.LogDebug(
                "Creating a new channel for integration flow with Id {FlowId}", flow.Id);

            var channelName = $"system/flows/{flow.Id}";
            var flowChannelOptions = new CloudEventsChannelOptions() { Name = channelName };
            flowChannelOptions.Components.AddRange(flow.Components);
            flowChannelOptions.Interceptors.AddRange(flow.Interceptors);

            var flowChannel = new CloudEventsChannel(flowChannelOptions);

            _channelManager.Add(flowChannel);

            _logger.LogDebug("Created new channel for flow {FlowId} with name {ChannelId}", flow.Id, channelName);

            if (sourceChannel == null && existingEsInstance != null)
            {
                _logger.LogDebug("Using event source instances {EsInstanceId} internal channel as the source channel for flow {FlowId}",
                    existingEsInstance.InternalChannelId, flow.Id);

                var esInstanceChannelName = existingEsInstance.InternalChannelId;
                sourceChannel = _channelManager.Get(esInstanceChannelName);
            }

            if (sourceChannel == null)
            {
                throw new UnknownIntegrationFlowSourceException();
            }

            if (sourceChannel.IsPubSub == false)
            {
                // TODO: We could actually add a new endpoint to the source channel to get around this issue
                throw new NotSupportedChannelTypeForIntegrationFlow();
            }
            
            sourceChannel.Subscribe(flowChannel);
            
            Add(flow);
        }

        public async Task<CloudEventsIntegrationFlow> Create(IntegrationFlowDefinition flowDefinition, string id = null, string description = null,
            object configuration = null)
        {
            _logger.LogInformation("Creating Integration Flow using registered flow with definition {Definition}", flowDefinition);

            var flowType = _integrationFlowProvider.Get(flowDefinition);
            
            CloudEventsIntegrationFlowBase flow;

            if (configuration != null)
            {
                flow = (CloudEventsIntegrationFlowBase)ActivatorUtilities.CreateInstance(_serviceProvider, flowType, new object[] { configuration });
            }
            else
            {
                flow = (CloudEventsIntegrationFlowBase)ActivatorUtilities.CreateInstance(_serviceProvider, flowType);
            }

            var result = await flow.Flow.Build(_serviceProvider);
            result.Id = id;
            result.Description = description;
            result.Configuration = configuration;

            return result;
        }

        public List<CloudEventsIntegrationFlow> List()
        {
            return this;
        }
    }
}
