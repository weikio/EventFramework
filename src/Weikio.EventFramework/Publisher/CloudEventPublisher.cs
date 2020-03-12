using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Extensions;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Configuration;

namespace Weikio.EventFramework.Publisher
{
    public class CloudEventPublisher : ICloudEventPublisher
    {
        private readonly ICloudEventGatewayManager _gatewayManager;
        private readonly EventFrameworkOptions _options;

        public CloudEventPublisher(ICloudEventGatewayManager gatewayManager, IOptions<EventFrameworkOptions> options)
        {
            _gatewayManager = gatewayManager;
            _options = options.Value;
        }

        public async Task<List<CloudEvent>> Publish(IList<object> objects, string eventTypeName = "", string id = "", Uri source = null,
            string gatewayName = GatewayName.Default)
        {
            if (objects == null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            var cloudEvents = new List<CloudEvent>();
            for (var index = 0; index < objects.Count; index++)
            {
                var obj = objects[index];

                var cloudEvent = CreateCloudEvent(obj, eventTypeName, "", source, new ICloudEventExtension[] { new IntegerSequenceExtension(index) });
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

            var cloudEvent = CreateCloudEvent(obj, eventTypeName, id, source);

            var result = await Publish(cloudEvent, gatewayName);

            return result;
        }

        public async Task<CloudEvent> Publish(CloudEvent cloudEvent)
        {
            var gatewayName = _options.DefaultGatewayName;

            return await Publish(cloudEvent, gatewayName);
        }
        
        public async Task<CloudEvent> Publish(CloudEvent cloudEvent, string gatewayName)
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

            await outgoingChannel.Send(cloudEvent);

            return cloudEvent;
        }
        
        private CloudEvent CreateCloudEvent(object obj, string eventTypeName, string id, Uri source, ICloudEventExtension[] extensions = null)
        {
            if (string.IsNullOrEmpty(eventTypeName))
            {
                eventTypeName = obj.GetType().Name;
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString();
            }

            if (source == null)
            {
                source = _options.DefaultSource;
            }

            var cloudEvent = new CloudEvent(eventTypeName, source, id, DateTime.UtcNow, extensions) { Data = obj };

            return cloudEvent;
        }
    }
}
