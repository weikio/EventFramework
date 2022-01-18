using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.EventFlow.Abstractions;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public class EventFlowBuilder : IEventFlowBuilder
    {
        private Action<EventSourceInstanceOptions> _configureEventSourceInstance;
        private Type _eventSourceType;
        public string Source { get; set; }

        public string Name { get; set; } = "flow_" + Guid.NewGuid();
        public Version Version { get; set; } = new Version(1, 0, 0);

        public string Description { get; set; } = "";

        public List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)> Interceptors { get; set; } =
            new List<(InterceptorTypeEnum InterceptorType, IChannelInterceptor Interceptor)>();

        public List<Func<ComponentFactoryContext, Task<CloudEventsComponent>>> Components { get; set; } =
            new List<Func<ComponentFactoryContext, Task<CloudEventsComponent>>>();

        public static EventFlowBuilder From()
        {
            var builder = new EventFlowBuilder();

            return builder;
        }

        public static IEventFlowBuilder From<TEventSourceType>(Action<EventSourceInstanceOptions> configureInstance = null)
        {
            var builder = new EventFlowBuilder { _configureEventSourceInstance = configureInstance, _eventSourceType = typeof(TEventSourceType) };

            return builder;
        }

        public static EventFlowBuilder From(string source)
        {
            var builder = new EventFlowBuilder { Source = source };

            return builder;
        }

        public EventFlowDefinition Definition { get; set; }

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
                ComponentFactories = Components
            };

            return Task.FromResult(integrationFlow);
        }
    }
}
