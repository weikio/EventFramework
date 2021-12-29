using System;

namespace Weikio.EventFramework.EventSource.Schedule
{
    public class ScheduleEvent
    {
        public DateTimeOffset DateTimeOffsetUtc { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset DateTimeOffsetLocal
        {
            get
            {
                return DateTimeOffsetUtc.ToLocalTime();
            }
        }
        
        public DateTimeOffset DateTimeUtc
        {
            get
            {
                return DateTimeOffsetUtc.UtcDateTime;
            }
        }
        
        public DateTimeOffset DateTimeLocal
        {
            get
            {
                return DateTimeOffsetUtc.LocalDateTime;
            }
        }
    }
}
