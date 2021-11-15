using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.Components;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.IntegrationFlow.CloudEvents
{
    public class IntegrationFlowBuilder : IBuilder<CloudEventsIntegrationFlow>
    {
        private ArrayList _flow = new ArrayList();
        private List<Func<IServiceProvider, Task<CloudEventsComponent>>> _components = new List<Func<IServiceProvider, Task<CloudEventsComponent>>>();
        private Action<EventSourceInstanceOptions> _configureEventSourceInstance;
        private Type _eventSourceType;
        public string Source { get; private set; }
        public string Id { get; private set; } = "flow_" + Guid.NewGuid();
        public string Description { get; private set; } = "";
        public object Configuration { get; private set; } = null;
        public List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)> Interceptors { get; set; } =
            new List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)>();

        public static IntegrationFlowBuilder From()
        {
            var builder = new IntegrationFlowBuilder();

            return builder;
        }

        public static IntegrationFlowBuilder From<TEventSourceType>(Action<EventSourceInstanceOptions> configureInstance = null)
        {
            var builder = new IntegrationFlowBuilder
            {
                _configureEventSourceInstance = configureInstance, 
                _eventSourceType = typeof(TEventSourceType)
            };

            return builder;
        }
        
        public static IntegrationFlowBuilder From(string source)
        {
            var builder = new IntegrationFlowBuilder
            {
                Source =  source
            };

            return builder;
        }

        public IntegrationFlowBuilder WithId(string id)
        {
            Id = id;

            return this;
        }
        
        public IntegrationFlowBuilder WithDescription(string description)
        {
            Description = description;

            return this;
        }
        
        public IntegrationFlowBuilder WithInterceptor(InterceptorTypeEnum interceptorType, IChannelInterceptor interceptor)
        {
            Interceptors.Add((interceptorType, interceptor));

            return this;
        }
        
        public IntegrationFlowBuilder WithConfiguration(object configuration)
        {
            Configuration = configuration;

            return this;
        }

        public async Task<CloudEventsIntegrationFlow> Build(IServiceProvider serviceProvider)
        {
            var result = new CloudEventsIntegrationFlow
            {
                Id = Id, 
                Description = Description, 
                ConfigureEventSourceInstanceOptions = _configureEventSourceInstance,
                EventSourceType = _eventSourceType,
                Source = Source,
                Configuration = Configuration,
                Interceptors = Interceptors
            };
            
            // Insert a component which adds a IntegrationFlowExtension to the event's attributes
            var extensionComponent = new AddExtensionComponent(ev => new EventFrameworkIntegrationFlowEventExtension(Id));
            
            result.Components.Add(extensionComponent);

            foreach (var componentBuilder in _components)
            {
                var component = await componentBuilder(serviceProvider);
                result.Components.Add(component);
            }
            
            return result;
        }

        public IntegrationFlowBuilder Register(CloudEventsComponent component)
        {
            _components.Add(provider => Task.FromResult(component));

            return this;
        }

        public IntegrationFlowBuilder Register(Func<IServiceProvider, Task<CloudEventsComponent>> componentBuilder)
        {
            _components.Add(componentBuilder);

            return this;
        }
    }
}
