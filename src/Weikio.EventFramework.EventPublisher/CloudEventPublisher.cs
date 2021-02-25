using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventGateway;

namespace Weikio.EventFramework.EventPublisher
{
    public class CloudEventPublisher : ICloudEventPublisher
    {
        private readonly ICloudEventGatewayManager _gatewayManager;
        private readonly ICloudEventCreator _cloudEventCreator;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CloudEventPublisher> _logger;
        private readonly ICloudEventChannelManager _cloudEventChannelManager;
        private readonly CloudEventPublisherOptions _options;

        public CloudEventPublisher(ICloudEventGatewayManager gatewayManager, IOptions<CloudEventPublisherOptions> options, 
            ICloudEventCreator cloudEventCreator, IServiceProvider serviceProvider, ILogger<CloudEventPublisher> logger, ICloudEventChannelManager cloudEventChannelManager)
        {
            _gatewayManager = gatewayManager;
            _cloudEventCreator = cloudEventCreator;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _cloudEventChannelManager = cloudEventChannelManager;
            _options = options.Value;
        }

        public async Task<CloudEvent> Publish(object obj, string eventTypeName = "", string id = "", Uri source = null,
            string gatewayName = GatewayName.Default)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
   
            var creationOptions = GetDefaultCloudEventCreationOptions();
            _options.ConfigureCloudEventCreationOptions(eventTypeName, obj, creationOptions, _serviceProvider);
            
            var cloudEvent = _cloudEventCreator.CreateCloudEvent(obj, eventTypeName, id, source, creationOptions: creationOptions);

            if (string.Equals(gatewayName, GatewayName.Default) && !string.IsNullOrWhiteSpace(_options.DefaultGatewayName))
            {
                gatewayName = _options.DefaultGatewayName;
            }
            
            var result = await Publish(cloudEvent, gatewayName);

            return result;
        }
        
        public async Task<List<CloudEvent>> Publish(IEnumerable objects, string eventTypeName = "", string id = "", Uri source = null,
            string gatewayName = GatewayName.Default)
        {
            if (objects == null)
            {
                throw new ArgumentNullException(nameof(objects));
            }
            
            if (string.Equals(gatewayName, GatewayName.Default))
            {
                gatewayName = _options.DefaultGatewayName;
            }

            var cloudEvents = new List<CloudEvent>();

            var index = 0;
            foreach (var obj in objects)
            {
                var creationOptions = GetDefaultCloudEventCreationOptions();
                _options.ConfigureCloudEventCreationOptions(eventTypeName, obj, creationOptions, _serviceProvider);

                var cloudEvent = _cloudEventCreator.CreateCloudEvent(obj, eventTypeName, "", source, new ICloudEventExtension[] { new IntegerSequenceExtension(index) }, creationOptions: creationOptions);
                cloudEvents.Add(cloudEvent);
                
                index += 1;
            }

            var result = new List<CloudEvent>(cloudEvents.Count);

            foreach (var cloudEvent in cloudEvents)
            {
                var publishedEvent = await Publish(cloudEvent, gatewayName);
                result.Add(publishedEvent);
            }

            return result;
        }

        private CloudEventCreationOptions GetDefaultCloudEventCreationOptions()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                // Get the default cloud event creation options for each publish
                var result = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<CloudEventCreationOptions>>().Value;

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get default cloud event creation options");

                throw;
            }
        }

        public async Task<CloudEvent> Publish(CloudEvent cloudEvent)
        {
            var gatewayName = _options.DefaultGatewayName;

            return await Publish(cloudEvent, gatewayName);
        }

        public virtual async Task<CloudEvent> Publish(CloudEvent cloudEvent, string gatewayName)
        {
            if (cloudEvent == null)
            {
                throw new ArgumentNullException(nameof(cloudEvent));
            }

            if (string.IsNullOrWhiteSpace(cloudEvent.Id))
            {
                cloudEvent.Id = Guid.NewGuid().ToString();
            }

            ICloudEventGateway gateway = null;

            try
            {
                gateway = _gatewayManager.Get(gatewayName);

                if (gateway == null)
                {
                    throw new UnknownGatewayException(gatewayName);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get gateway with name {GatewayName}", gatewayName);

                //throw;
            }

            IChannel outgoingChannel = gateway?.OutgoingChannel;

            if (!string.IsNullOrWhiteSpace(_options.DefaultChannelName))
            {
                outgoingChannel = _cloudEventChannelManager.Get(_options.DefaultChannelName);
            }

            if (outgoingChannel == null)
            {
                throw new OutgoingChannelNotSupportedException();
            }

            if (string.IsNullOrEmpty(cloudEvent.Id))
            {
                cloudEvent.Id = Guid.NewGuid().ToString();
            }

            var beforePublish = _options.OnBeforePublish;

            if (beforePublish != null)
            {
                cloudEvent = await beforePublish(_serviceProvider, cloudEvent);
            }
            
            await outgoingChannel.Send(cloudEvent);
            
            _logger.LogDebug("Published cloud event {CloudEvent} to channel {Channel}", cloudEvent, outgoingChannel.Name);

            return cloudEvent;
        }
    }
}
