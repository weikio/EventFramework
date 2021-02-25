using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventGateway;

namespace Weikio.EventFramework.EventPublisher
{
    public class CloudEventPublisherOptions
    {
        public string DefaultGatewayName { get; set; } = GatewayName.Default;
        public string DefaultChannelName { get; set; } = ChannelName.Default;

        public Dictionary<string, Action<CloudEventCreationOptions>> TypedCloudEventCreationOptions { get; set; } =
            new Dictionary<string, Action<CloudEventCreationOptions>>();

        public Action<CloudEventCreationOptions> ConfigureDefaultCloudEventCreationOptions { get; set; } = options =>
        {
        };

        public Func<IServiceProvider, CloudEvent, Task<CloudEvent>> OnBeforePublish = (provider, cloudEvent) => Task.FromResult(cloudEvent);
    }

    public static class CloudEventPublisherOptionsExtensions
    {
        public static void ConfigureCloudEventCreationOptions(this CloudEventPublisherOptions publisherOptions, string eventType, object obj,
            CloudEventCreationOptions creationOptions, IServiceProvider serviceProvider)
        {
            publisherOptions.ConfigureDefaultCloudEventCreationOptions(creationOptions);

            if (publisherOptions.TypedCloudEventCreationOptions?.Any() != true)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(eventType))
            {
                eventType = creationOptions.GetEventTypeName(creationOptions, serviceProvider, obj);
            }

            if (publisherOptions.TypedCloudEventCreationOptions.ContainsKey(eventType) == false)
            {
                return;
            }

            var optionConfigure = publisherOptions.TypedCloudEventCreationOptions[eventType];

            optionConfigure(creationOptions);
        }
    }
}
