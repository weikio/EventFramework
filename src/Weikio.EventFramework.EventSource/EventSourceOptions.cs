using System;
using Quartz;

namespace Weikio.EventFramework.EventSource
{
    public class EventSourceOptions
    {
        public Func<string, string> EventSourceInstanceChannelNameFactory { get; set; } = instanceId => $"system/eventsourceinstances/{instanceId}";

        public Action<SchedulerBuilder.PersistentStoreOptions> ConfigureStatePersistentStore { get; set; } = options =>
        {
        };

        public static class Defaults
        {
            public static Action<SchedulerBuilder.PersistentStoreOptions> ConfigureStatePersistentStore { get; set; } = s =>
            {
                s.UseProperties = true;
                s.RetryInterval = TimeSpan.FromSeconds(15);
                s.UseJsonSerializer();
                s.UseClustering(c =>
                {
                    c.CheckinMisfireThreshold = TimeSpan.FromSeconds(20);
                    c.CheckinInterval = TimeSpan.FromSeconds(10);
                });
            };
        }
    }
}
