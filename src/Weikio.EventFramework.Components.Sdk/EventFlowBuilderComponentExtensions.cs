using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Weikio.EventFramework.Channels.Abstractions;
using Weikio.EventFramework.Channels.CloudEvents;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public static class EventFlowBuilderComponentExtensions
    {
        public static IEventFlowBuilder Component(this IEventFlowBuilder builder, Func<CloudEvent, CloudEvent> func, Predicate<CloudEvent> predicate = null)
        {
            var cloudEventsComponent = new CloudEventsComponent(func, predicate);

            Task<CloudEventsComponent> Get(ComponentFactoryContext context)
            {
                return Task.FromResult(cloudEventsComponent);
            }

            return builder.Component(Get);
        }

        public static IEventFlowBuilder Component(this IEventFlowBuilder builder, Func<CloudEvent, Task<CloudEvent>> func,
            Predicate<CloudEvent> predicate = null)
        {
            var cloudEventsComponent = new CloudEventsComponent(func, predicate);

            Task<CloudEventsComponent> Get(ComponentFactoryContext context)
            {
                return Task.FromResult(cloudEventsComponent);
            }

            return builder.Component(Get);
        }

        public static IEventFlowBuilder Component(this IEventFlowBuilder builder, Func<ComponentFactoryContext, Task<CloudEventsComponent>> componentBuilder)
        {
            builder.Components.Add(componentBuilder);

            return builder;
        }

        public static IEventFlowBuilder Component(this IEventFlowBuilder builder, IComponentBuilder componentBuilder)
        {
            builder.Components.Add(componentBuilder.Build);

            return builder;
        }

        public static IEventFlowBuilder Component<TComponentBuilder>(this IEventFlowBuilder builder, Action<TComponentBuilder> configure = null)
            where TComponentBuilder : IComponentBuilder
        {
            builder.Components.Add(new TypeComponentBuilder(typeof(TComponentBuilder), configure).Build);

            return builder;
        }
    }
    
    
    public class TypeComponentBuilder : IComponentBuilder
    {
        private readonly Type _type;
        private readonly MulticastDelegate _configure;

        public TypeComponentBuilder(Type type, MulticastDelegate configure)
        {
            _type = type;
            _configure = configure;
        }

        public Task<CloudEventsComponent> Build(ComponentFactoryContext context)
        {
            var builder = (IComponentBuilder) ActivatorUtilities.CreateInstance(context.ServiceProvider, _type);

            if (_configure != null)
            {
                _configure.DynamicInvoke(builder);
            }

            return builder.Build(context);
        }
    }
}
