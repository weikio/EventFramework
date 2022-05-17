using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Weikio.EventFramework.Channels.CloudEvents
{
    public class CloudEventsChannelStartupHandler : IHostedService
    {
        private readonly ILogger<CloudEventsChannelStartupHandler> _logger;
        private readonly IEnumerable<ChannelInstanceOptions> _channelInstances;
        private readonly ICloudEventsChannelManager _cloudEventsChannelManager;
        private readonly ICloudEventsChannelBuilder _cloudEventsChannelBuilder;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<CloudEventsChannelBuilder> _channelBuilders;

        public CloudEventsChannelStartupHandler(ILogger<CloudEventsChannelStartupHandler> logger, IEnumerable<ChannelInstanceOptions> channelInstances,
            ICloudEventsChannelManager cloudEventsChannelManager, ICloudEventsChannelBuilder cloudEventsChannelBuilder, IServiceProvider serviceProvider, IEnumerable<CloudEventsChannelBuilder> channelBuilders)
        {
            _logger = logger;
            _channelInstances = channelInstances;
            _cloudEventsChannelManager = cloudEventsChannelManager;
            _cloudEventsChannelBuilder = cloudEventsChannelBuilder;
            _serviceProvider = serviceProvider;
            _channelBuilders = channelBuilders;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Starting cloud event channels. Channels are initialized on startup based on the registered ChannelInstanceOptions");

                var channelInstances = _channelInstances.ToList();
                var channelBuilders = _channelBuilders.ToList();

                if (channelInstances.Count + channelBuilders.Count  <= 0)
                {
                    _logger.LogDebug("No channels to create on system startup");

                    return;
                }

                _logger.LogTrace("Found {InitialInstanceCount} channels to create on system startup", channelInstances.Count);

                var createdChannels = new List<CloudEventsChannel>();

                foreach (var channelInstance in channelInstances)
                {
                    CloudEventsChannelOptions options;

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var provider = scope.ServiceProvider;

                        var optionsAccessor = provider.GetService<IOptionsSnapshot<CloudEventsChannelOptions>>();
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
                
                _logger.LogTrace("Found {BuilderCount} channel builders from which to create a channel on system startup", channelBuilders.Count);

                foreach (var channelBuilder in channelBuilders)
                {
                    var channel = await channelBuilder.Build(_serviceProvider);
                    _cloudEventsChannelManager.Add(channel);
                    
                    createdChannels.Add(channel);
                }

                _logger.LogInformation("Created {InitialChannelCount} channels on system startup", createdChannels.Count);

                foreach (var createdChannel in createdChannels)
                {
                    _logger.LogDebug("Channel: {CreatedChannelDetails}", createdChannel);
                }

                foreach (var channelBuilder in channelBuilders)
                {
                    var subscriber = _cloudEventsChannelManager.Get(channelBuilder.Name);

                    foreach (var subscribeTo in channelBuilder.Subscriptions)
                    {
                        // TODO: Error handling
                        var publisher = _cloudEventsChannelManager.Get(subscribeTo);
                        publisher.Subscribe(subscriber);
                        
                        _logger.LogDebug("Channel {Subscriber} subscribed to publisher channel {Publisher}", channelBuilder.Name, subscribeTo);
                    }
                }
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
