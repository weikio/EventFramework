using System;
using System.Globalization;

namespace Weikio.EventFramework.EventSource
{
    public class PollingSchedule
    {
        public PollingSchedule(Guid id, TimeSpan? interval, string cronExpression, Guid parentId) : this(id.ToString(), interval, cronExpression, parentId)
        {
        }
        
        public PollingSchedule(string id, TimeSpan? interval, string cronExpression, Guid parentId)
        {
            Id = id;
            CronExpression = cronExpression;
            ParentId = parentId;
            Interval = interval;
        }
        
        public string Id { get; }
        public Guid ParentId { get; }
        public TimeSpan? Interval { get; }
        public string CronExpression { get; }
    }
}
