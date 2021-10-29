using System;
using System.Threading.Tasks;
using Weikio.EventFramework.EventSource;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class CloudEventsIntegrationFlowManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventSourceInstanceManager _eventSourceInstanceManager;
        private readonly IEventSourceProvider _eventSourceProvider;

        public CloudEventsIntegrationFlowManager(IServiceProvider serviceProvider, IEventSourceInstanceManager eventSourceInstanceManager,
            IEventSourceProvider eventSourceProvider)
        {
            _serviceProvider = serviceProvider;
            _eventSourceInstanceManager = eventSourceInstanceManager;
            _eventSourceProvider = eventSourceProvider;
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

            if (flow.ConfigureEventSourceInstanceOptions != null)
            {
                flow.ConfigureEventSourceInstanceOptions(esOptions);
            }
            else
            {
                esOptions.Autostart = true;
                esOptions.Id = "testflow";
                esOptions.PollingFrequency = TimeSpan.FromSeconds(2);
            }

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
                isDefinitionKnown = false;
            }

            if (isDefinitionKnown == false)
            {
                // Register if needed
                var catalog = new PluginFrameworkEventSourceCatalog(typePluginCatalog);
                await _eventSourceProvider.AddCatalog(catalog);
            }

            esOptions.ConfigureChannel = options =>
            {
                options.Components.AddRange(flow.Components);
            };

            await _eventSourceInstanceManager.Create(esOptions);
        }
    }
}
