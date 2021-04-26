using System;
using Weikio.EventFramework.EventPublisher;

namespace Weikio.EventFramework.EventSource.Abstractions
{
    public class EventSourceInstanceOptions
    {
        public EventSourceDefinition EventSourceDefinition { get; set; }
        public TimeSpan? PollingFrequency { get; set; }
        public string CronExpression { get; set; }
        public MulticastDelegate Configure { get; set; }
        public bool Autostart { get; set; }
        public bool RunOnce { get; set; }
        public object Configuration { get; set; }
        public string TargetChannelName { get; set; }
        public Guid? Id { get; set; }

        public Action<CloudEventPublisherOptions> ConfigurePublisherOptions = options =>
        {

        };
    }
}
