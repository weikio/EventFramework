using System;
using System.Threading.Tasks;
using Weikio.EventFramework.Channels;
using Weikio.EventFramework.EventCreator;
using Weikio.EventFramework.EventGateway;
using Weikio.EventFramework.EventPublisher;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public static class EventSourceInstanceManagerExtensions
    {
        public static Task<Guid> Create(this IEventSourceInstanceManager manager, string name, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Action<CloudEventPublisherOptions> configurePublisherOptions = null,
            Action<CloudEventCreationOptions> configureDefaultCloudEventCreationOptions = null, object configuration = null, Guid? id = null)
        {
            return Create(manager, name, Version.Parse("1.0.0.0"), pollingFrequency, cronExpression, configure, configurePublisherOptions,
                configureDefaultCloudEventCreationOptions, configuration, null, id);
        }

        public static Task<Guid> Create(this IEventSourceInstanceManager manager, Abstractions.EventSource eventSource, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Action<CloudEventPublisherOptions> configurePublisherOptions = null,
            Action<CloudEventCreationOptions> configureDefaultCloudEventCreationOptions = null, object configuration = null, Guid? id = null)
        {
            return Create(manager, eventSource.EventSourceDefinition.Name, eventSource.EventSourceDefinition.Version, pollingFrequency, cronExpression,
                configure, configurePublisherOptions, configureDefaultCloudEventCreationOptions, configuration, null, id);
        }

        public static Task<Guid> Create(this IEventSourceInstanceManager manager, string name, Version version, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, Action<CloudEventPublisherOptions> configurePublisherOptions = null,
            Action<CloudEventCreationOptions> configureDefaultCloudEventCreationOptions = null, object configuration = null, string channelName = null,
            Guid? id = null)
        {
            var eventSourceDefinition = new EventSourceDefinition(name, version);

            if (configurePublisherOptions == null && configureDefaultCloudEventCreationOptions != null)
            {
                configurePublisherOptions = publisherOptions =>
                {
                    publisherOptions.ConfigureDefaultCloudEventCreationOptions = configureDefaultCloudEventCreationOptions;
                };
            }
            else if (configureDefaultCloudEventCreationOptions != null)
            {
                configurePublisherOptions = publisherOptions =>
                {
                    publisherOptions.ConfigureDefaultCloudEventCreationOptions += configureDefaultCloudEventCreationOptions;
                };
            }

            var options = new EventSourceInstanceOptions()
            {
                Autostart = false,
                RunOnce = false,
                Configure = configure,
                ConfigurePublisherOptions = configurePublisherOptions,
                CronExpression = cronExpression,
                EventSourceDefinition = eventSourceDefinition,
                PollingFrequency = pollingFrequency,
                Configuration = configuration,
                TargetChannelName = channelName,
                Id = id
            };

            return manager.Create(options);
        }
    }
}
