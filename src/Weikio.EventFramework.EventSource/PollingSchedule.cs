using System;
using System.Globalization;

namespace Weikio.EventFramework.EventSource
{
    public class PollingSchedule
    {
        public PollingSchedule(Guid id, TimeSpan? interval, string cronExpression) : this(id.ToString(), interval, cronExpression)
        {
        }
        
        public PollingSchedule(string id, TimeSpan? interval, string cronExpression)
        {
            Id = id;
            CronExpression = cronExpression;
            Interval = interval;
        }
        
        public string Id { get; }
        public TimeSpan? Interval { get; }
        public string CronExpression { get; }
    }
}
