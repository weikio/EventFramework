using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CloudNative.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            services.TryAddSingleton<IChannelBuilder, CloudEventsChannelBuilder>();
            services.TryAddSingleton<ICloudEventsChannelManager, DefaultCloudEventsChannelManager>();
            services.AddHostedService<CloudEventsChannelStartupHandler>();
            services.TryAddSingleton<ICloudEventsChannelBuilder, DefaultCloudEventsChannelBuilder>();

            return services;
        }

        public static IEventFrameworkBuilder AddChannel(this IEventFrameworkBuilder builder, string name,
            Action<IServiceProvider, CloudEventsDataflowChannelOptions> configure = null)
        {
            builder.Services.AddCloudEventDataflowChannels();
            builder.Services.AddChannel(name, configure);

            return builder;
        }

        public static IServiceCollection AddChannel(this IServiceCollection services, string name,
            Action<IServiceProvider, CloudEventsDataflowChannelOptions> configure = null)
        {
            services.AddSingleton(new ChannelInstanceOptions() { Configure = configure, Name = name });

            return services;
        }
    }

    public interface ICloudEventsChannelBuilder
    {
        CloudEventsChannel Create(CloudEventsDataflowChannelOptions options);
    }

    public class DefaultCloudEventsChannelBuilder : ICloudEventsChannelBuilder
    {
        public CloudEventsChannel Create(CloudEventsDataflowChannelOptions options)
        {
            return new CloudEventsChannel(options);
        }
    }

    public class ChannelInstanceOptions
    {
        public string Name { get; set; }
        public Action<IServiceProvider, CloudEventsDataflowChannelOptions> Configure { get; set; }
    }

    public class CloudEventsChannelStartupHandler : IHostedService
    {
        private readonly ILogger<CloudEventsChannelStartupHandler> _logger;
        private readonly IEnumerable<ChannelInstanceOptions> _channelInstances;
        private readonly ICloudEventsChannelManager _cloudEventsChannelManager;
        private readonly ICloudEventsChannelBuilder _cloudEventsChannelBuilder;
        private readonly IServiceProvider _serviceProvider;

        public CloudEventsChannelStartupHandler(ILogger<CloudEventsChannelStartupHandler> logger, IEnumerable<ChannelInstanceOptions> channelInstances,
            ICloudEventsChannelManager cloudEventsChannelManager, ICloudEventsChannelBuilder cloudEventsChannelBuilder, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _channelInstances = channelInstances;
            _cloudEventsChannelManager = cloudEventsChannelManager;
            _cloudEventsChannelBuilder = cloudEventsChannelBuilder;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Starting cloud event channels. Channels are initialized on startup based on the registered ChannelInstanceOptions");

                var channelInstances = _channelInstances.ToList();

                if (channelInstances.Count < 0)
                {
                    _logger.LogDebug("No channels to create on system startup");

                    return Task.CompletedTask;
                }

                _logger.LogTrace("Found {InitialInstanceCount} channels to create on system startup", channelInstances.Count);

                var createdChannels = new List<CloudEventsChannel>();

                foreach (var channelInstance in channelInstances)
                {
                    CloudEventsDataflowChannelOptions options;

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var provider = scope.ServiceProvider;

                        var optionsAccessor = provider.GetService<IOptionsSnapshot<CloudEventsDataflowChannelOptions>>();
                        options = optionsAccessor.Value;
                    }
                    
                    if (channelInstance.Configure != null)
                    {
                        channelInstance.Configure(_serviceProvider, options);
                    }

                    options.Name = channelInstance.Name;

                    var channel = _cloudEventsChannelBuilder.Create(options);
                    _cloudEventsChannelManager.Add(channel);

                    createdChannels.Add(channel);
                }

                _logger.LogInformation("Created {InitialChannelCount} channels on system startup", createdChannels.Count);

                foreach (var createdChannel in createdChannels)
                {
                    _logger.LogDebug("Channel: {CreatedChannelDetails}", createdChannel);
                }

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create channels on startup");

                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
