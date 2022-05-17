using System;
using System.Threading.Tasks.Dataflow;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.EventFramework.Abstractions.DependencyInjection;
using Weikio.EventFramework.Channels.Dataflow;

namespace Weikio.EventFramework.Channels.CloudEvents
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
            services.TryAddSingleton<IChannelBuilder, CloudEventsChannelBuilder>();
            services.TryAddSingleton<ICloudEventsChannelManager, DefaultCloudEventsChannelManager>();
            services.AddHostedService<CloudEventsChannelStartupHandler>();
            services.TryAddSingleton<ICloudEventsChannelBuilder, DefaultCloudEventsChannelBuilder>();

            return services;
        }

        public static IEventFrameworkBuilder AddChannel(this IEventFrameworkBuilder builder, string name,
            Action<IServiceProvider, CloudEventsChannelOptions> configure = null)
        {
            builder.Services.AddChannel(name, configure);

            return builder;
        }

        public static IEventFrameworkBuilder AddChannel(this IEventFrameworkBuilder builder, CloudEventsChannelBuilder channelBuilder)
        {
            builder.Services.AddChannel(channelBuilder);

            return builder;
        }
        
        public static IServiceCollection AddChannel(this IServiceCollection services, CloudEventsChannelBuilder channelBuilder)
        {
            services.AddSingleton(channelBuilder);
            
            return services;
        }

        public static IServiceCollection AddChannel(this IServiceCollection services, string name,
            Action<IServiceProvider, CloudEventsChannelOptions> configure = null)
        {
            services.AddCloudEventDataflowChannels();
            services.AddSingleton(new ChannelInstanceOptions() { Configure = configure, Name = name });

            return services;
        }
    }
}
