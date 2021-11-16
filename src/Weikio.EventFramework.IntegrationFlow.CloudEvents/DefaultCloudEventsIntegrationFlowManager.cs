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
    internal class DefaultCloudEventsIntegrationFlowManager : List<IntegrationFlowInstance>, ICloudEventsIntegrationFlowManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventSourceInstanceManager _eventSourceInstanceManager;
        private readonly IEventSourceProvider _eventSourceProvider;
        private readonly ILogger<DefaultCloudEventsIntegrationFlowManager> _logger;
        private readonly ICloudEventsChannelManager _channelManager;
        private readonly IntegrationFlowProvider _integrationFlowProvider;
        private readonly IIntegrationFlowInstanceFactory _instanceFactory;

        public DefaultCloudEventsIntegrationFlowManager(IServiceProvider serviceProvider, IEventSourceInstanceManager eventSourceInstanceManager,
            IEventSourceProvider eventSourceProvider, ILogger<DefaultCloudEventsIntegrationFlowManager> logger, 
            ICloudEventsChannelManager channelManager, IntegrationFlowProvider integrationFlowProvider, IIntegrationFlowInstanceFactory instanceFactory)
        {
            _serviceProvider = serviceProvider;
            _eventSourceInstanceManager = eventSourceInstanceManager;
            _eventSourceProvider = eventSourceProvider;
            _logger = logger;
            _channelManager = channelManager;
            _integrationFlowProvider = integrationFlowProvider;
            _instanceFactory = instanceFactory;
        }

        public async Task Execute(IntegrationFlowInstance flowInstance)
        {
            _logger.LogInformation("Executing integration flow with ID {Id}", flowInstance.Id);

            // Create event source instance and a channel based on the input
            // But only create event source instance if needed. If source is defined, try to find existing event source instance or channel
            if (string.IsNullOrWhiteSpace(flowInstance.Source))
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
                    options.Components.AddRange(flowInstance.Components);
                    options.Interceptors.AddRange(flowInstance.Interceptors);
                };

                await _eventSourceInstanceManager.Create(esOptions);

                _logger.LogDebug("New Event source with Id {EsId} created for Integration flow with Id {Id}", esOptions.Id, flowInstance.Id);

                Add(flowInstance);
                
                return;
            }

            _logger.LogDebug(
                "Source {Source} is defined for integration flow with ID {Id}. Trying to find existing channel or event source instance based on source",
                flowInstance.Source, flowInstance.Id);
            var existingEsInstance = _eventSourceInstanceManager.Get(flowInstance.Source);

            CloudEventsChannel sourceChannel = null;

            if (existingEsInstance == null)
            {
                _logger.LogDebug("No existing Event Source Instance found with Id {Source}. Trying to find an existing channel", flowInstance.Source);

                try
                {
                    sourceChannel = _channelManager.Get(flowInstance.Source);
                }
                catch (Exception ex) when (ex is UnknownChannelException || ex is NoChannelsConfiguredException)
                {
                    _logger.LogDebug("Could not locate channel using name {Source} for Flow with Id {FlowId}", flowInstance.Source, flowInstance.Id);

                    throw new UnknownIntegrationFlowSourceException();
                }
                
                _logger.LogDebug("Found existing channel {ChannelName} as a source for integration flow with Id {Source}", flowInstance.Source, flowInstance.Source);
            }

            _logger.LogDebug(
                "Creating a new channel for integration flow with Id {FlowId}", flowInstance.Id);

            var channelName = $"system/flows/{flowInstance.Id}";
            var flowChannelOptions = new CloudEventsChannelOptions() { Name = channelName };
            flowChannelOptions.Components.AddRange(flowInstance.Components);
            flowChannelOptions.Interceptors.AddRange(flowInstance.Interceptors);

            var flowChannel = new CloudEventsChannel(flowChannelOptions);

            _channelManager.Add(flowChannel);

            _logger.LogDebug("Created new channel for flow {FlowId} with name {ChannelId}", flowInstance.Id, channelName);

            if (sourceChannel == null && existingEsInstance != null)
            {
                _logger.LogDebug("Using event source instances {EsInstanceId} internal channel as the source channel for flow {FlowId}",
                    existingEsInstance.InternalChannelId, flowInstance.Id);

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
            
            Add(flowInstance);
        }

        public async Task<IntegrationFlowInstance> Create(IntegrationFlowDefinition flowDefinition, string id = null, string description = null,
            object configuration = null)
        {
            _logger.LogInformation("Creating Integration Flow using registered flow with definition {Definition}", flowDefinition);

            var flowType = _integrationFlowProvider.Get(flowDefinition);
            //
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

            var integrationFlow = new Abstractions.IntegrationFlow(flowDefinition, flowType, null);
            var options = new IntegrationFlowInstanceOptions() { Id = id, Configuration = configuration, Description = description };

            var result = await _instanceFactory.Create(integrationFlow, options);// await flow.Flow.Build(_serviceProvider);
            // result.Id = id;
            // result.Description = description;
            // result.Configuration = configuration;

            return result;
        }

        public List<IntegrationFlowInstance> List()
        {
            return this;
        }
    }
}
