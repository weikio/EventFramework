using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventFlow.CloudEvents.Components;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public static class EventFlowBuilderExtensions
    {
        public static IEventFlowBuilder Channel(this IEventFlowBuilder builder, string channelName, Predicate<CloudEvent> predicate = null)
        {
            var component = new ChannelComponentBuilder(channelName, predicate);
            
            builder.Component(component);

            return builder;
        }

        public static IEventFlowBuilder Transform(this IEventFlowBuilder builder, Func<CloudEvent, CloudEvent> transform, Predicate<CloudEvent> predicate = null)
        {
            var component = new CloudEventsComponent(transform, predicate);
            builder.Component(component);

            return builder;
        }

        public static IEventFlowBuilder Filter(this IEventFlowBuilder builder, Func<CloudEvent, Filter> filter)
        {
            var componentBuilder = new FilterComponentBuilder(filter);
            builder.Component(componentBuilder.Build);

            return builder;
        }

        public static IEventFlowBuilder Filter(this IEventFlowBuilder builder, Predicate<CloudEvent> filter)
        {
            var componentBuilder = new FilterComponentBuilder(filter);
            builder.Component(componentBuilder.Build);

            return builder;
        }

        public static IEventFlowBuilder Handle<THandlerType>(this IEventFlowBuilder builder, Predicate<CloudEvent> predicate = null,
            Action<THandlerType> configure = null)
        {
            var componentBuilder = new HandlerComponentBuilder(null, null, typeof(THandlerType), configure);
            builder.Component(componentBuilder);

            return builder;
        }

        public static IEventFlowBuilder Handle(this IEventFlowBuilder builder, Action<CloudEvent> handler, Predicate<CloudEvent> predicate = null)
        {
            if (predicate == null)
            {
                predicate = ev => true;
            }
            
            var componentBuilder = new HandlerComponentBuilder((ev, provider) =>
            {
                handler(ev);
                return Task.CompletedTask;
            }, ev => Task.FromResult(predicate(ev)));
            
            builder.Component(componentBuilder);

            return builder;
        }
        
        public static IEventFlowBuilder Handle(this IEventFlowBuilder builder, Action<CloudEvent, IServiceProvider> handler, Predicate<CloudEvent> predicate = null)
        {
            if (predicate == null)
            {
                predicate = ev => true;
            }
            
            var componentBuilder = new HandlerComponentBuilder((ev, provider) =>
            {
                handler(ev, provider);
                return Task.CompletedTask;
            }, ev => Task.FromResult(predicate(ev)));
            
            builder.Component(componentBuilder);

            return builder;
        }
        
        public static IEventFlowBuilder Handle(this IEventFlowBuilder builder, Func<CloudEvent, Task> handler, Predicate<CloudEvent> predicate = null)
        {
            if (predicate == null)
            {
                predicate = ev => true;
            }

            var taskPredicate = new Func<CloudEvent, Task<bool>>(ev => Task.FromResult(predicate(ev)));

            return builder.Handle(handler, taskPredicate);
        }
        
        public static IEventFlowBuilder Handle(this IEventFlowBuilder builder, Func<CloudEvent, Task> handler,
            Func<CloudEvent, Task<bool>> predicate = null, Type handlerType = null, MulticastDelegate configureHandler = null)
        {
            var componentBuilder = new HandlerComponentBuilder((ev, provider) =>
            {
                handler(ev);
                return Task.CompletedTask;
            }, predicate, handlerType, configureHandler);
            
            builder.Component(componentBuilder);

            return builder;
        }
    }
}
