using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        public DefaultIntegrationFlowInstanceFactory(IServiceProvider serviceProvider, ILogger<DefaultIntegrationFlowInstanceFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<IntegrationFlowInstance> Create(Abstractions.IntegrationFlow integrationFlow, IntegrationFlowInstanceOptions options)
        {
            // Insert a component which adds a IntegrationFlowExtension to the event's attributes
            var extensionComponent = new AddExtensionComponent(ev => new EventFrameworkIntegrationFlowEventExtension(options.Id));
            options.Components.Add(extensionComponent);

            foreach (var componentFactory in options.ComponentFactories)
            {
                var context = new ComponentFactoryContext(_serviceProvider, integrationFlow, options, options.Components.Count);
                
                var component = await componentFactory(context);
                options.Components.Add(component);
            }
            
            // Insert an endpoint which transfer the event to the desired channel if event has attribute eventFramework_integrationFlow_endpoint
            options.Endpoints.Add(new CloudEventsEndpoint(async ev =>
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
                await channel.Send(ev);
            }));
            
            
            var result = new IntegrationFlowInstance(integrationFlow, options);
            
            return result;
        }
    }

    public class ComponentFactoryContext
    {
        public IServiceProvider ServiceProvider { get; }
        public Abstractions.IntegrationFlow IntegrationFlow { get; }
        public IntegrationFlowInstanceOptions Options { get;  }
        public int CurrentComponentIndex { get;  }
        
        public ComponentFactoryContext(IServiceProvider serviceProvider, Abstractions.IntegrationFlow integrationFlow, IntegrationFlowInstanceOptions options, int currentComponentIndex)
        {
            ServiceProvider = serviceProvider;
            IntegrationFlow = integrationFlow;
            Options = options;
            CurrentComponentIndex = currentComponentIndex;
        }
    }
}
