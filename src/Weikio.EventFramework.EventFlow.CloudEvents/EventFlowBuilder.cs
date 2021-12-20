using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.EventFlow.Abstractions;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class EventFlowBuilder 
    {
        private ArrayList _flow = new ArrayList();
        private List<Func<ComponentFactoryContext, Task<CloudEventsComponent>>> _components = new List<Func<ComponentFactoryContext, Task<CloudEventsComponent>>>();
        private Action<EventSourceInstanceOptions> _configureEventSourceInstance;
        private Type _eventSourceType;
        public string Source { get; private set; }
        // public string Id { get; private set; } = "flowinstance_" + Guid.NewGuid();
        public string Name { get; private set; } = "flow_" + Guid.NewGuid();
        public Version Version { get; private set; } = new Version(1, 0, 0);

        public string Description { get; private set; } = "";
        public object Configuration { get; private set; } = null;
        public List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)> Interceptors { get; set; } =
            new List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)>();

        public static EventFlowBuilder From()
        {
            var builder = new EventFlowBuilder();

            return builder;
        }

        public static EventFlowBuilder From<TEventSourceType>(Action<EventSourceInstanceOptions> configureInstance = null)
        {
            var builder = new EventFlowBuilder
            {
                _configureEventSourceInstance = configureInstance, 
                _eventSourceType = typeof(TEventSourceType)
            };

            return builder;
        }
        
        public static EventFlowBuilder From(string source)
        {
            var builder = new EventFlowBuilder
            {
                Source =  source
            };

            return builder;
        }

        public EventFlowBuilder WithName(string name)
        {
            Name = name;

            return this;
        }
        
        public EventFlowBuilder WithId(string id)
        {
            // Id = id;
        
            return this;
        }
        
        public EventFlowBuilder WithDescription(string description)
        {
            Description = description;

            return this;
        }

        public EventFlowBuilder WithDefinition(EventFlowDefinition definition)
        {
            Definition = definition;

            return this;
        }

        public EventFlowDefinition Definition { get; set; }

        public EventFlowBuilder WithInterceptor(InterceptorTypeEnum interceptorType, IChannelInterceptor interceptor)
        {
            Interceptors.Add((interceptorType, interceptor));

            return this;
        }
        
        public EventFlowBuilder WithConfiguration(object configuration)
        {
            Configuration = configuration;

            return this;
        }

        public EventFlowBuilder WithSource(string source)
        {
            Source = source;

            return this;
        }

        public EventFlowBuilder WithVersion(string version)
        {
            WithVersion(System.Version.Parse(version));

            return this;
        }

        public EventFlowBuilder WithVersion(Version version)
        {
            Version = version;

            return this;
        }
        
        public Task<EventFlow> Build(IServiceProvider serviceProvider)
        {
            if (Definition == null)
            {
                Definition = new EventFlowDefinition { Name = Name, Version = Version, Description = Description };
            }
            
            var integrationFlow = new EventFlow(Definition, _eventSourceType, Source)
            {
                ConfigureEventSourceInstanceOptions = _configureEventSourceInstance,
                Interceptors = Interceptors,
                ComponentFactories = _components
            };

            return Task.FromResult(integrationFlow);
        }

        public EventFlowBuilder Register(CloudEventsComponent component)
        {
            _components.Add(provider => Task.FromResult(component));

            return this;
        }

        public EventFlowBuilder Register(Func<ComponentFactoryContext, Task<CloudEventsComponent>> componentBuilder)
        {
            _components.Add(componentBuilder);

            return this;
        }
    }
}
