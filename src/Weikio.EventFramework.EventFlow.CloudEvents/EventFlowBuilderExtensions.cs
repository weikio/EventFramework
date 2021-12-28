using System;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.Channels.CloudEvents;
using Weikio.EventFramework.EventFlow.CloudEvents.Components;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public static class EventFlowBuilderExtensions
    {
        public static EventFlowBuilder Channel(this EventFlowBuilder builder, string channelName, Predicate<CloudEvent> predicate = null)
        {
            var component = new ChannelComponentBuilder(channelName, predicate);
            
            builder.Component(component);

            return builder;
        }

        public static EventFlowBuilder Transform(this EventFlowBuilder builder, Func<CloudEvent, CloudEvent> transform, Predicate<CloudEvent> predicate = null)
        {
            var component = new CloudEventsComponent(transform, predicate);
            builder.Component(component);

            return builder;
        }

        public static EventFlowBuilder Filter(this EventFlowBuilder builder, Func<CloudEvent, Filter> filter)
        {
            var componentBuilder = new FilterComponentBuilder(filter);
            builder.Component(componentBuilder.Build);

            return builder;
        }

        public static EventFlowBuilder Filter(this EventFlowBuilder builder, Predicate<CloudEvent> filter)
        {
            var componentBuilder = new FilterComponentBuilder(filter);
            builder.Component(componentBuilder.Build);

            return builder;
        }

        public static EventFlowBuilder Handle<THandlerType>(this EventFlowBuilder builder, Predicate<CloudEvent> predicate = null,
            Action<THandlerType> configure = null)
        {
            var componentBuilder = new HandlerComponentBuilder(null, null, typeof(THandlerType), configure);
            builder.Component(componentBuilder);

            return builder;
        }

        public static EventFlowBuilder Handle(this EventFlowBuilder builder, Action<CloudEvent> handler, Predicate<CloudEvent> predicate = null)
        {
            if (predicate == null)
            {
                predicate = ev => true;
            }
            
            var componentBuilder = new HandlerComponentBuilder(ev =>
            {
                handler(ev);
                return Task.CompletedTask;
            }, ev => Task.FromResult(predicate(ev)));
            
            builder.Component(componentBuilder);

            return builder;
        }

        public static EventFlowBuilder Handle(this EventFlowBuilder builder, Func<CloudEvent, Task> handler, Predicate<CloudEvent> predicate = null)
        {
            if (predicate == null)
            {
                predicate = ev => true;
            }

            var taskPredicate = new Func<CloudEvent, Task<bool>>(ev => Task.FromResult(predicate(ev)));

            return builder.Handle(handler, taskPredicate);
        }

        public static EventFlowBuilder Handle(this EventFlowBuilder builder, Func<CloudEvent, Task> handler,
            Func<CloudEvent, Task<bool>> predicate = null, Type handlerType = null, MulticastDelegate configureHandler = null)
        {
            var componentBuilder = new HandlerComponentBuilder(ev =>
            {
                handler(ev);
                return Task.CompletedTask;
            }, predicate, handlerType, configureHandler);
            
            builder.Component(componentBuilder);

            return builder;
        }
    }
}
