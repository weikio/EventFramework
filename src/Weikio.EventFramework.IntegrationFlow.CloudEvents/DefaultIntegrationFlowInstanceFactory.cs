using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Components;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class DefaultIntegrationFlowInstanceFactory : IIntegrationFlowInstanceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DefaultIntegrationFlowInstanceFactory> _logger;
        private readonly IOptions<IntegrationFlowChannelDefaultComponents> _options;

        public DefaultIntegrationFlowInstanceFactory(IServiceProvider serviceProvider, ILogger<DefaultIntegrationFlowInstanceFactory> logger, IOptions<IntegrationFlowChannelDefaultComponents> options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options;
        }

        public async Task<IntegrationFlowInstance> Create(IntegrationFlow integrationFlow, IntegrationFlowInstanceOptions options)
        {
            // Component channel names are created based on the index, so 0, 1, 2 etc.
            // Is there any use for the fact that we use index numbers? Maybe just give Guid to the component and use that as channel name.
            // Or, why we create the components at this point? Why doesn't the DefaultCloudEventsIntegrationFlowManager create them?
            // Or maybe the channels should be created here
            for (var index = 0; index < integrationFlow.ComponentFactories.Count; index++)
            {
                var componentFactory = integrationFlow.ComponentFactories[index];
                
                // TODO: Find a place for this
                var componentChannelName = $"system/flows/{options.Id}/componentchannels/{integrationFlow.Components.Count}";

                var hasNextComponent = integrationFlow.ComponentFactories.Count > index + 1;

                string nextComponentChannelName = null;

                if (hasNextComponent)
                {
                    // TODO: Find a place for this
                    nextComponentChannelName = $"system/flows/{options.Id}/componentchannels/{ integrationFlow.Components.Count + 1}";
                }
                
                var context = new ComponentFactoryContext(_serviceProvider, integrationFlow, options, integrationFlow.Components.Count, 
                    componentChannelName, 
                    nextComponentChannelName);

                var component = await componentFactory(context);
                integrationFlow.Components.Add(component);
            }

            // Insert an endpoint which transfer the event to the desired channel if event has attribute eventFramework_integrationFlow_endpoint
            integrationFlow.Endpoints.Add(new CloudEventsEndpoint(async ev =>
            {
                var attrs = ev.GetAttributes();

                if (attrs.ContainsKey(EventFrameworkIntegrationFlowEndpointEventExtension.EventFrameworkIntegrationFlowEndpointAttributeName) == false)
                {
                    return;
                }

                var targetChannel = attrs[EventFrameworkIntegrationFlowEndpointEventExtension.EventFrameworkIntegrationFlowEndpointAttributeName] as string;

                if (string.IsNullOrWhiteSpace(targetChannel))
                {
                    return;
                }

                var channel = _serviceProvider.GetRequiredService<IChannelManager>().Get(targetChannel);
                
                // After a transfer, we want to remove the attribute so that the event doesn't get stuck in a loop
                attrs.Remove(EventFrameworkIntegrationFlowEndpointEventExtension.EventFrameworkIntegrationFlowEndpointAttributeName);
                
                await channel.Send(ev);
            }));

            var result = new IntegrationFlowInstance(integrationFlow, options);

            return result;
        }
    }

    public class IntegrationFlowChannelDefaultComponents
    {
        public Func<IntegrationFlow, IntegrationFlowInstanceOptions, List<CloudEventsComponent>> ComponentsFactory { get; set; } =
            (flow, options) =>
            {
                var result = new List<CloudEventsComponent> { new AddExtensionComponent(ev => new EventFrameworkIntegrationFlowEventExtension(options.Id)) };

                return result;
            };
    }

    public class ComponentFactoryContext
    {
        public IServiceProvider ServiceProvider { get; }
        public IntegrationFlow IntegrationFlow { get; }
        public IntegrationFlowInstanceOptions Options { get; }
        public int CurrentComponentIndex { get; }
        public string CurrentComponentChannelName { get; set; }
        public string NextComponentChannelName { get; set; }

        public ComponentFactoryContext(IServiceProvider serviceProvider, IntegrationFlow integrationFlow, IntegrationFlowInstanceOptions options,
            int currentComponentIndex, string currentComponentChannelName, string nextComponentChannelName)
        {
            ServiceProvider = serviceProvider;
            IntegrationFlow = integrationFlow;
            Options = options;
            CurrentComponentIndex = currentComponentIndex;
            CurrentComponentChannelName = currentComponentChannelName;
            NextComponentChannelName = nextComponentChannelName;
        }
    }
}
