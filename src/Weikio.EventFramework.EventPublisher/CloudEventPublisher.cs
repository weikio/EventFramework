using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Extensions;
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
        private readonly CloudEventPublisherOptions _options;

        public CloudEventPublisher(ICloudEventGatewayManager gatewayManager, IOptions<CloudEventPublisherOptions> options, ICloudEventCreator cloudEventCreator, IServiceProvider serviceProvider)
        {
            _gatewayManager = gatewayManager;
            _cloudEventCreator = cloudEventCreator;
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        public async Task<List<CloudEvent>> Publish(IList<object> objects, string eventTypeName = "", string id = "", Uri source = null,
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

            for (var index = 0; index < objects.Count; index++)
            {
                var obj = objects[index];

                var cloudEvent = _cloudEventCreator.CreateCloudEvent(obj, eventTypeName, "", source, new ICloudEventExtension[] { new IntegerSequenceExtension(index) });
                cloudEvents.Add(cloudEvent);
            }

            var result = new List<CloudEvent>(cloudEvents.Count);

            foreach (var cloudEvent in cloudEvents)
            {
                var publishedEvent = await Publish(cloudEvent, gatewayName);
                result.Add(publishedEvent);
            }

            return result;
        }

        public async Task<CloudEvent> Publish(object obj, string eventTypeName = "", string id = "", Uri source = null,
            string gatewayName = GatewayName.Default)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var cloudEvent = _cloudEventCreator.CreateCloudEvent(obj, eventTypeName, id, source);

            if (string.Equals(gatewayName, GatewayName.Default) && !string.IsNullOrWhiteSpace(_options.DefaultGatewayName))
            {
                gatewayName = _options.DefaultGatewayName;
            }
            
            var result = await Publish(cloudEvent, gatewayName);

            return result;
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

            var gateway = _gatewayManager.Get(gatewayName);

            if (gateway == null)
            {
                throw new UnknownGatewayException(gatewayName);
            }

            var outgoingChannel = gateway.OutgoingChannel;

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

            return cloudEvent;
        }
    }
}
