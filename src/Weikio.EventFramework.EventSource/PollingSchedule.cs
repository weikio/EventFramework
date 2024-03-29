﻿using System;
using System.Globalization;
using Weikio.EventFramework.EventSource.Abstractions;
using Weikio.EventFramework.EventSource.EventSourceWrapping;

namespace Weikio.EventFramework.EventSource
{
    public class PollingSchedule
    {
        public PollingSchedule(Guid id, TimeSpan? interval, string cronExpression, EventSourceInstance eventSourceInstance) : this(id.ToString(), interval, cronExpression, eventSourceInstance)
        {
        }
        
        public PollingSchedule(string id, TimeSpan? interval, string cronExpression, EventSourceInstance eventSourceInstance)
        {
            Id = id;
            CronExpression = cronExpression;
            EventSourceInstance = eventSourceInstance;
            Interval = interval;
        }
        
        public string Id { get; }
        public TimeSpan? Interval { get; }
        public string CronExpression { get; }
        public EventSourceInstance EventSourceInstance { get; }
    }
}
