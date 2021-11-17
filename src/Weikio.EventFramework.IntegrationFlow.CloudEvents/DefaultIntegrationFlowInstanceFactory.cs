using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
