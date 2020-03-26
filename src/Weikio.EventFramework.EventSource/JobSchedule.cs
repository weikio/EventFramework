using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Quartz;

namespace Weikio.EventFramework.EventSource
{
    public class JobSchedule
    {
        public JobSchedule(Type jobType, TimeSpan interval)
        {
            JobType = jobType;
            Interval = interval;
        }
        
        public JobSchedule(Type jobType, string cronExpression)
        {
            JobType = jobType;
            CronExpression = cronExpression;
        }
        
        public JobSchedule(Type jobType, TimeSpan? interval, string cronExpression)
        {
            JobType = jobType;
            CronExpression = cronExpression;
            Interval = interval;
        }

        public JobSchedule(Func<IServiceProvider, Task> factory, TimeSpan? interval, string cronExpression)
        {
            CronExpression = cronExpression;
            Interval = interval;
            Factory = factory;
        }

        public JobSchedule(Func<IServiceProvider, IJobExecutionContext, Task> action, TimeSpan? interval, string cronExpression)
        {
            Action = action;
            CronExpression = cronExpression;
            Interval = interval;
        }

        public Func<IServiceProvider, IJobExecutionContext, Task> Action { get; set; }
        public Func<IServiceProvider, Task> Factory { get; }
        public Type JobType { get; }
        public TimeSpan? Interval { get; }
        public string CronExpression { get; }
        public MulticastDelegate Configure { get; set; }
    }
}
