using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public interface ICloudEventsIntegrationFlowManager
    {
        Task Execute(CloudEventsIntegrationFlow flow);
    }

    internal class DefaultCloudEventsIntegrationFlowManager : ICloudEventsIntegrationFlowManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventSourceInstanceManager _eventSourceInstanceManager;
        private readonly IEventSourceProvider _eventSourceProvider;
        private readonly ILogger<DefaultCloudEventsIntegrationFlowManager> _logger;
        private readonly ICloudEventsChannelManager _channelManager;

        public DefaultCloudEventsIntegrationFlowManager(IServiceProvider serviceProvider, IEventSourceInstanceManager eventSourceInstanceManager,
            IEventSourceProvider eventSourceProvider, ILogger<DefaultCloudEventsIntegrationFlowManager> logger, ICloudEventsChannelManager channelManager)
        {
            _serviceProvider = serviceProvider;
            _eventSourceInstanceManager = eventSourceInstanceManager;
            _eventSourceProvider = eventSourceProvider;
            _logger = logger;
            _channelManager = channelManager;
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
                };

                await _eventSourceInstanceManager.Create(esOptions);

                _logger.LogDebug("New Event source with Id {EsId} created for Integration flow with Id {Id}", esOptions.Id, flow.Id);

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
        }
    }
}
