using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.EventFramework.Abstractions.DependencyInjection;

namespace Weikio.EventFramework.Channels.Dataflow.CloudEvents
{
    public static class ServiceCollectionExtensions
    {
        public static IEventFrameworkBuilder AddCloudEventDataflowChannels(this IEventFrameworkBuilder builder)
        {
            AddCloudEventDataflowChannels(builder.Services);

            return builder;
        }

        public static IServiceCollection AddCloudEventDataflowChannels(this IServiceCollection services)
        {
            services.AddDataflowChannels();
            services.TryAddSingleton<IChannelBuilder, CloudEventsDataflowChannelBuilder>();

            return services;
        }
    }
}
