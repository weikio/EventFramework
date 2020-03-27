using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weikio.EventFramework.Abstractions;
using Weikio.EventFramework.Configuration;

namespace Weikio.EventFramework.Publisher
{
    public class CloudEventPublisherOptions
    {
        public string EventTypeName { get; set; }
        public CloudEventsSpecVersion SpecVersion { get; set; } = CloudEventsSpecVersion.V1_0;

        public Func<CloudEventPublisherOptions, IServiceProvider, object, string> GetEventTypeName { get; set; } = (options, provider, o) =>
        {
            if (!string.IsNullOrWhiteSpace(options.EventTypeName))
            {
                return options.EventTypeName;
            }

            return o.GetType().Name;
        };

        public string Subject { get; set; } = string.Empty;
        public string DataContentType { get; set; } = "Application/Json";

        public Func<CloudEventPublisherOptions, IServiceProvider, object, string> GetDataContentType { get; set; } =
            (options, provider, o) => options.DataContentType;

        public Func<CloudEventPublisherOptions, IServiceProvider, object, string> GetSubject { get; set; } = (options, provider, o) => options.Subject;
        public Func<CloudEventPublisherOptions, IServiceProvider, object, string> GetId { get; set; } = (options, provider, o) => Guid.NewGuid().ToString();

        public Func<CloudEventPublisherOptions, IServiceProvider, object, ICloudEventExtension[]> GetExtensions { get; set; } =
            (options, provider, o) => Array.Empty<ICloudEventExtension>();
    }

    public class CloudEventPublisher : ICloudEventPublisher
    {
        private readonly ICloudEventGatewayManager _gatewayManager;
        private readonly IOptionsMonitor<CloudEventPublisherOptions> _optionsMonitor;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CloudEventPublisher> _logger;
        private readonly ICloudEventCreator _cloudEventCreator;
        private readonly EventFrameworkOptions _options;

        public CloudEventPublisher(ICloudEventGatewayManager gatewayManager, IOptions<EventFrameworkOptions> options,
            IOptionsMonitor<CloudEventPublisherOptions> optionsMonitor, IServiceProvider serviceProvider, ILogger<CloudEventPublisher> logger, ICloudEventCreator cloudEventCreator)
        {
            _gatewayManager = gatewayManager;
            _optionsMonitor = optionsMonitor;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _cloudEventCreator = cloudEventCreator;
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

            if (string.Equals(gatewayName, GatewayName.Default))
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
    }
}
