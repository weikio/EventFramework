using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Channels.Dataflow;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.Components;
using Weikio.EventFramework.EventFlow.Abstractions;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    internal class DefaultCloudEventFlowManager : List<EventFlowInstance>, ICloudEventFlowManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventSourceInstanceManager _eventSourceInstanceManager;
        private readonly IEventSourceProvider _eventSourceProvider;
        private readonly ILogger<DefaultCloudEventFlowManager> _logger;
        private readonly ICloudEventsChannelManager _channelManager;
        private readonly EventFlowProvider _eventFlowProvider;
        private readonly IEventFlowInstanceFactory _instanceFactory;
        private readonly IOptions<EventFlowChannelDefaultComponents> _options;
        private readonly EventFlowOptions _eventFlowOptions;

        public DefaultCloudEventFlowManager(IServiceProvider serviceProvider, IEventSourceInstanceManager eventSourceInstanceManager,
            IEventSourceProvider eventSourceProvider, ILogger<DefaultCloudEventFlowManager> logger,
            ICloudEventsChannelManager channelManager, EventFlowProvider eventFlowProvider, IEventFlowInstanceFactory instanceFactory,
            IOptions<EventFlowChannelDefaultComponents> options, IOptions<EventFlowOptions> eventFlowOptions)
        {
            _serviceProvider = serviceProvider;
            _eventSourceInstanceManager = eventSourceInstanceManager;
            _eventSourceProvider = eventSourceProvider;
            _logger = logger;
            _channelManager = channelManager;
            _eventFlowProvider = eventFlowProvider;
            _instanceFactory = instanceFactory;
            _options = options;
            _eventFlowOptions = eventFlowOptions.Value;
        }

        public async Task<EventFlowInstance> Execute(EventFlow eventFlow)
        {
            var options = new EventFlowInstanceOptions() { Id = "flowinstance_" + Guid.NewGuid() };

            return await Execute(eventFlow, options);
        }

        public async Task<EventFlowInstance> Execute(EventFlow eventFlow, EventFlowInstanceOptions instanceOptions)
        {
            var instance = await _instanceFactory.Create(eventFlow, instanceOptions);

            return await Execute(instance);
        }

        public async Task<EventFlowInstance> Execute(EventFlowInstance flowInstance)
        {
            _logger.LogInformation("Executing integration flow with ID {Id}", flowInstance.Id);
            
            // Create event source instance and a channel based on the input
            // But only create event source instance if needed. If source is defined, try to find existing event source instance or channel
            if (string.IsNullOrWhiteSpace(flowInstance.Source) && flowInstance.EventSourceType != null)
            {
                _logger.LogDebug("Integration flow with ID {Id} requires a new event source", flowInstance.Id);

                var esOptions = new EventSourceInstanceOptions();
                esOptions.Id = flowInstance.Id;
                esOptions.Autostart = true;
                esOptions.PollingFrequency = TimeSpan.FromSeconds(2);

                if (flowInstance.ConfigureEventSourceInstanceOptions != null)
                {
                    flowInstance.ConfigureEventSourceInstanceOptions(esOptions);
                }

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

                return flowInstance;
            }

            _logger.LogDebug(
                "Source {Source} is defined for integration flow with ID {Id}. Trying to find existing channel, event source instance or event flow based on source",
                flowInstance.Source, flowInstance.Id);
            var existingEsInstance = _eventSourceInstanceManager.Get(flowInstance.Source);
            var existingFlow = this.FirstOrDefault(x => string.Equals(x.Id, flowInstance.Source));

            if (existingEsInstance == null && existingFlow == null && !string.IsNullOrWhiteSpace(flowInstance.Source))
            {
                existingFlow = this.FirstOrDefault(x => string.Equals(x.FlowDefinition.Name, flowInstance.Source));
            }

            CloudEventsChannel sourceChannel = null;

            if (existingEsInstance == null && existingFlow == null && !string.IsNullOrWhiteSpace(flowInstance.Source))
            {
                _logger.LogDebug("No existing Event Source Instance or Flow found with Id {Source}. Trying to find an existing channel", flowInstance.Source);

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

                    if (_eventFlowOptions.AutoCreateSourceChannel)
                    {
                        _logger.LogDebug("Based on options creating new source channel {ChannelName} automatically for Flow with Id {FlowId}",
                            flowInstance.Source, flowInstance.Id);
                        var newSourceChannel = new CloudEventsChannel(flowInstance.Source);
                        _channelManager.Add(newSourceChannel);

                        sourceChannel = newSourceChannel;
                    }

                    else
                    {
                        throw new UnknownIntegrationFlowSourceException();
                    }
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

            if (sourceChannel == null && existingFlow != null)
            {
                _logger.LogDebug("Using Even Flow {EventFlowId} as the source for flow {FlowId}",
                    existingFlow.Id, flowInstance.Id);

                var existingFlowOutputChannelName = existingFlow.OutputChannel;
                sourceChannel = _channelManager.Get(existingFlowOutputChannelName);
            }

            var inputChannel = _channelManager.Get(flowInstance.InputChannel);
            
            if (sourceChannel != null)
            {
                if (sourceChannel.IsPubSub == false)
                {
                    // TODO: We could actually add a new endpoint to the source channel to get around this issue
                    throw new NotSupportedChannelTypeForEventFlow();
                }

                sourceChannel.Subscribe(inputChannel);
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

            return flowInstance;
        }

        public async Task<EventFlowInstance> Create(EventFlowDefinition flowDefinition, string id = null, string description = null,
            object configuration = null)
        {
            _logger.LogInformation("Creating Integration Flow using registered flow with definition {Definition}", flowDefinition);

            var flowType = _eventFlowProvider.Get(flowDefinition);

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

        public List<EventFlowInstance> List()
        {
            return this;
        }

        public EventFlowInstance Get(string id)
        {
            var result = this.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.InvariantCultureIgnoreCase));

            if (result != null)
            {
                return result;
            }

            throw new UnknownEventFlowInstance(id,
                $"Could not find Event Flow Instance with ID: {id}.{Environment.NewLine}{Environment.NewLine}Available flows:{Environment.NewLine}{string.Join(",", this.Select(x => x.Id))}");
        }
    }

    public class CurrentFlowChannelInterceptor : IChannelInterceptor
    {
        private readonly string _channel;

        public CurrentFlowChannelInterceptor(string channel)
        {
            _channel = channel;
        }

        public Task<object> Intercept(object obj)
        {
            var ev = (CloudEvent)obj;
            var ext = new EventFrameworkEventFlowCurrentChanneEventExtension(_channel);

            ext.Attach(ev);

            return Task.FromResult<object>(ev);
        }
    }
}
