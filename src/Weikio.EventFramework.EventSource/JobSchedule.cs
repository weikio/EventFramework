using System;

namespace Weikio.EventFramework.EventSource
{
    public class JobSchedule
    {
        public JobSchedule(Guid id, TimeSpan? interval, string cronExpression)
        {
            Id = id;
            CronExpression = cronExpression;
            Interval = interval;
        }
        public Guid Id { get; }
        public TimeSpan? Interval { get; }
        public string CronExpression { get; }
    }
}
