using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.Channels.Dataflow;

namespace Weikio.EventFramework.Channels
{
    public static class ServiceCollectionExtensions
    {
        public static IEventFrameworkBuilder AddChannels(this IEventFrameworkBuilder builder)
        {
            AddChannels(builder.Services);

            return builder;
        }

        public static IServiceCollection AddChannels(this IServiceCollection services)
        {
            services.TryAddSingleton<ICloudEventChannelManager, DefaultCloudEventChannelManager>();

            return services;
        }
    }
}
