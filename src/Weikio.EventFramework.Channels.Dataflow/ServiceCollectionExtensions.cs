using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.EventFramework.Abstractions.DependencyInjection;

namespace Weikio.EventFramework.Channels.Dataflow
{
    public static class ServiceCollectionExtensions
    {
        public static IEventFrameworkBuilder AddDataflowChannels(this IEventFrameworkBuilder builder)
        {
            AddDataflowChannels(builder.Services);

            return builder;
        }

        public static IServiceCollection AddDataflowChannels(this IServiceCollection services)
        {
            services.AddChannels();
            services.TryAddSingleton<IChannelBuilder, DataflowChannelBuilder>();

            return services;
        }
    }
}
