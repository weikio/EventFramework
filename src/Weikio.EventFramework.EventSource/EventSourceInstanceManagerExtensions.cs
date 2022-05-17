using System;
using System.Threading.Tasks;
using Weikio.EventFramework.EventSource.Abstractions;

namespace Weikio.EventFramework.EventSource
{
    public static class EventSourceInstanceManagerExtensions
    {
        public static Task<string> Create(this IEventSourceInstanceManager manager, string name, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, object configuration = null, string id = null)
        {
            return Create(manager, name, Version.Parse("1.0.0.0"), pollingFrequency, cronExpression, configure, configuration, null, id);
        }

        public static Task<string> Create(this IEventSourceInstanceManager manager, Abstractions.EventSource eventSource, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, object configuration = null, string id = null)
        {
            return Create(manager, eventSource.EventSourceDefinition.Name, eventSource.EventSourceDefinition.Version, pollingFrequency, cronExpression,
                configure, configuration, null, id);
        }

        public static Task<string> Create(this IEventSourceInstanceManager manager, string name, Version version, TimeSpan? pollingFrequency = null,
            string cronExpression = null, MulticastDelegate configure = null, object configuration = null, string channelName = null,
            string id = null)
        {
            var eventSourceDefinition = new EventSourceDefinition(name, version);

            var options = new EventSourceInstanceOptions()
            {
                Autostart = false,
                RunOnce = false,
                Configure = configure,
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
