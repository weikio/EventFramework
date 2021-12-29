﻿using System;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.EventSource.SDK;

namespace Weikio.EventFramework.EventSource.Files
{
    public static class EventFrameworkBuilderExtensions
    {
        public static IEventFrameworkBuilder AddFileEventSource(this IEventFrameworkBuilder builder,
            Action<EventSourceInstanceOptions> configureInstance = null)
        {
            builder.AddEventSource<FileEventSource>(configureInstance, typeof(FileEventSourceConfiguration));

            return builder;
        } 
    }
}
