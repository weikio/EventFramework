using System;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.EventSource.SDK;

namespace Weikio.EventFramework.EventSource.Http
{
    public static class EventFrameworkBuilderHttpCloudEventSourceExtensions
    {
        public static IEventFrameworkBuilder AddHttpCloudEventSource(this IEventFrameworkBuilder builder,
            string route)
        {
            var conf = new HttpCloudEventSourceConfiguration() { Route = route };

            Action<EventSourceInstanceOptions> configureInstance = options =>
            {
                options.Autostart = true;
                options.Id = "http";
                options.Configuration = conf;
            };

            var services = builder.Services;

            services.AddEventSource<HttpCloudEventSource>(configureInstance, typeof(HttpCloudEventSourceConfiguration));

            return builder;
        }

        public static IEventFrameworkBuilder AddHttpCloudEventSource(this IEventFrameworkBuilder builder,
            Action<EventSourceInstanceOptions> configureInstance = null)
        {
            var services = builder.Services;

            services.AddEventSource<HttpCloudEventSource>(configureInstance, typeof(HttpCloudEventSourceConfiguration));

            return builder;
        }
    }
}
