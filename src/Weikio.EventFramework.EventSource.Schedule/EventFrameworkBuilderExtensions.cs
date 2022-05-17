using System;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.EventSource.SDK;

namespace Weikio.EventFramework.EventSource.Schedule
{
    public static class EventFrameworkBuilderExtensions
    {
        public static IEventFrameworkBuilder AddScheduleEventSource(this IEventFrameworkBuilder builder,
            Action<EventSourceInstanceOptions> configureInstance = null)
        {
            builder.AddEventSource<ScheduleEventSource>(configureInstance);

            return builder;
        } 
    }
}
