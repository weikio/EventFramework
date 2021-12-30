using System;
using Weikio.EventFramework.Channels.Dataflow.Abstractions;
using Weikio.EventFramework.EventFlow.Abstractions;

namespace Weikio.EventFramework.EventFlow.CloudEvents
{
    public static class EventFlowBuilderCommonExtensions
    {
        public static IEventFlowBuilder WithName(this IEventFlowBuilder builder, string name)
        {
            builder.Name = name;

            return builder;
        }

        public static IEventFlowBuilder WithDescription(this IEventFlowBuilder builder, string description)
        {
            builder.Description = description;

            return builder;
        }

        public static IEventFlowBuilder WithDefinition(this IEventFlowBuilder builder, EventFlowDefinition definition)
        {
            builder.Definition = definition;

            return builder;
        }

        public static IEventFlowBuilder WithVersion(this IEventFlowBuilder builder, string version)
        {
            builder.WithVersion(System.Version.Parse(version));

            return builder;
        }

        public static IEventFlowBuilder WithVersion(this IEventFlowBuilder builder, Version version)
        {
            builder.Version = version;

            return builder;
        }

        public static IEventFlowBuilder WithInterceptor(this IEventFlowBuilder builder, InterceptorTypeEnum interceptorType, IChannelInterceptor interceptor)
        {
            builder.Interceptors.Add((interceptorType, interceptor));

            return builder;
        }

        public static IEventFlowBuilder WithSource(this IEventFlowBuilder builder, string source)
        {
            builder.Source = source;

            return builder;
        }
    }
}
